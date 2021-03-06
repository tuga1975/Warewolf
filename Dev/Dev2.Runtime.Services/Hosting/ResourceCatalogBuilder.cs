/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using ChinhDo.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Hosting
{
    class ResourceBuilderTO
    {
        internal string FilePath;
        internal FileStream FileStream;
    }

    public class ResourceCatalogBuilder
    {
        private readonly List<IResource> _resources = new List<IResource>();
        private readonly HashSet<Guid> _addedResources = new HashSet<Guid>();
        private readonly IResourceUpgrader _resourceUpgrader;
        private readonly List<DuplicateResource> _duplicateResources = new List<DuplicateResource>();
        private readonly object _addLock = new object();
        readonly List<string> _convertToBiteExtension = new List<string>();


        public ResourceCatalogBuilder(IResourceUpgrader resourceUpgrader)
        {
            _resourceUpgrader = resourceUpgrader;
        }
        public ResourceCatalogBuilder()
        {
            _resourceUpgrader = ResourceUpgraderFactory.GetUpgrader();
        }

        public IList<IResource> ResourceList => _resources;
        public List<DuplicateResource> DuplicateResources => _duplicateResources;


        public void TryBuildCatalogFromWorkspace(string workspacePath, params string[] folders)
        {
            if (string.IsNullOrEmpty(workspacePath))
            {
                throw new ArgumentNullException("workspacePath");
            }

            if (folders == null)
            {
                throw new ArgumentNullException("folders");
            }

            if (folders.Length == 0 || !Directory.Exists(workspacePath))
            {
                return;
            }

            var streams = new List<ResourceBuilderTO>();

            try
            {
                BuildCatalogFromWorkspace(workspacePath, folders, streams);
            }
            finally
            {
                // Close all FileStream instances in a finally block after the tasks are complete. 
                // If each FileStream was instead created in a using statement, the FileStream 
                // might be disposed of before the task was complete
                foreach (var stream in streams)
                {
                    stream.FileStream.Close();
                }
                UpdateExtensions(_convertToBiteExtension);
            }
        }

        private void BuildCatalogFromWorkspace(string workspacePath, string[] folders, List<ResourceBuilderTO> streams)
        {
            foreach (var path in folders.Where(f => !string.IsNullOrEmpty(f)).Select(f => Path.Combine(workspacePath, f)))
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var files = DirectoryHelper.GetFilesByExtensions(path, ".xml", ".bite");
                foreach (var file in files)
                {

                    var fa = File.GetAttributes(file);

                    if ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        Dev2Logger.Info("Removed READONLY Flag from [ " + file + " ]", GlobalConstants.WarewolfInfo);
                        File.SetAttributes(file, FileAttributes.Normal);
                    }

                    // Use the FileStream class, which has an option that causes asynchronous I/O to occur at the operating system level.  
                    // In many cases, this will avoid blocking a ThreadPool thread.  
                    var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
                    streams.Add(new ResourceBuilderTO { FilePath = file, FileStream = sourceStream });

                }
            }

            // Use the parallel task library to process file system ;)
            IList<Type> allTypes = new List<Type>();
            var connectionTypeName = typeof(Connection).Name;
            var dropBoxSourceName = typeof(DropBoxSource).Name;
            var sharepointSourceName = typeof(SharepointSource).Name;
            var dbType = typeof(DbSource).Name;
            try
            {
                var resourceBaseType = typeof(IResourceSource);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var types = assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => resourceBaseType.IsAssignableFrom(p));
                allTypes = types as IList<Type> ?? types.ToList();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorLoadingTypes, e, GlobalConstants.WarewolfError);
            }
            streams.ForEach(currentItem =>
            {

                XElement xml = null;
                try
                {
                    xml = XElement.Load(currentItem.FileStream);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("Resource [ " + currentItem.FilePath + " ] caused " + e.Message, GlobalConstants.WarewolfError);
                }

                var result = xml?.ToStringBuilder();

                var isValid = result != null && HostSecurityProvider.Instance.VerifyXml(result);
                var typeName = xml?.AttributeSafe("Type");
                if (isValid)
                {
                    //TODO: Remove this after V1 is released. All will be updated.
                    #region old typing to be removed after V1
                    if (!IsWarewolfResource(xml))
                    {
                        return;
                    }
                    if (typeName == "Unknown")
                    {
                        var servertype = xml.AttributeSafe("ResourceType");
                        if (servertype != null && servertype == dbType)
                        {
                            xml.SetAttributeValue("Type", dbType);
                            typeName = dbType;
                        }
                    }

                    if (typeName == "Dev2Server" || typeName == "Server" || typeName == "ServerSource")
                    {
                        xml.SetAttributeValue("Type", connectionTypeName);
                        typeName = connectionTypeName;
                    }

                    if (typeName == "OauthSource")
                    {
                        xml.SetAttributeValue("Type", dropBoxSourceName);
                        typeName = dropBoxSourceName;
                    }
                    if (typeName == "SharepointServerSource")
                    {
                        xml.SetAttributeValue("Type", sharepointSourceName);
                        typeName = sharepointSourceName;
                    }
                    #endregion

                    Type type = null;
                    if (allTypes.Count != 0)
                    {
                        type = allTypes.FirstOrDefault(type1 => type1.Name == typeName);
                    }
                    Resource resource;
                    if (type != null)
                    {
                        resource = (Resource)Activator.CreateInstance(type, xml);
                    }
                    else
                    {
                        resource = new Resource(xml);
                    }
                    if (currentItem.FilePath.EndsWith(".xml"))
                    {
                        _convertToBiteExtension.Add(currentItem.FilePath);
                        resource.FilePath = currentItem.FilePath.Replace(".xml", ".bite");
                    }
                    else
                    {
                        resource.FilePath = currentItem.FilePath;
                    }
                    xml = _resourceUpgrader.UpgradeResource(xml, Assembly.GetExecutingAssembly().GetName().Version, a =>
                    {

                        var fileManager = new TxFileManager();
                        using (TransactionScope tx = new TransactionScope())
                        {
                            try
                            {

                                var updateXml = a.ToStringBuilder();
                                var signedXml = HostSecurityProvider.Instance.SignXml(updateXml);
                                signedXml.WriteToFile(currentItem.FilePath, Encoding.UTF8, fileManager);
                                tx.Complete();
                            }
                            catch
                            {
                                try
                                {
                                    Transaction.Current.Rollback();
                                }
                                catch (Exception err)
                                {
                                    Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                                }
                                throw;
                            }
                        }

                    });
                    if (resource.IsUpgraded)
                    {
                        // Must close the source stream first and then add a new target stream 
                        // otherwise the file will be remain locked
                        currentItem.FileStream.Close();

                        xml = resource.UpgradeXml(xml, resource);

                        var updateXml = xml.ToStringBuilder();
                        var signedXml = HostSecurityProvider.Instance.SignXml(updateXml);
                        var fileManager = new TxFileManager();
                        using (TransactionScope tx = new TransactionScope())
                        {
                            try
                            {
                                signedXml.WriteToFile(currentItem.FilePath, Encoding.UTF8, fileManager);
                                tx.Complete();
                            }
                            catch
                            {
                                Transaction.Current.Rollback();
                                throw;
                            }
                        }
                    }

                    lock (_addLock)
                    {
                        AddResource(resource, currentItem.FilePath);
                    }
                }
                else
                {
                    Dev2Logger.Debug(string.Format("'{0}' wasn't loaded because it isn't signed or has modified since it was signed.", currentItem.FilePath), GlobalConstants.WarewolfDebug);
                }
            });
        }

        private void UpdateExtensions(List<string> extensionsToUpdateToBite)
        {
            foreach (var item in extensionsToUpdateToBite)
            {
                var updatedFile = String.Empty;
                updatedFile = Path.ChangeExtension(item, ".bite");
                if (File.Exists(updatedFile) && File.Exists(item))
                {
                    File.Delete(updatedFile);
                    File.Move(item, updatedFile);
                }
                else
                {
                    File.Move(item, updatedFile);
                }
            }
        }

        private bool IsWarewolfResource(XElement xml)
        {
            var resourceType = xml.AttributeSafe("ResourceType");
            var type = xml.AttributeSafe("Type");
            var action = xml.Descendants("Action").FirstOrDefault();
            var actionResourceType = action?.AttributeSafe("ResourceType");
            var actionType = action?.AttributeSafe("Type");
            if (string.IsNullOrEmpty(resourceType)
                && string.IsNullOrEmpty(type)
                && string.IsNullOrEmpty(actionResourceType)
                && string.IsNullOrEmpty(actionType))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="res">The res.</param>
        /// <param name="filePath">The file path.</param>
        private void AddResource(IResource res, string filePath)
        {
            if (!filePath.Contains("VersionControl"))
            {
                if (!_addedResources.Contains(res.ResourceID))
                {
                    _resources.Add(res);
                    _addedResources.Add(res.ResourceID);
                }
                else
                {
                    var dupRes = _resources.Find(c => c.ResourceID == res.ResourceID);
                    if (dupRes != null)
                    {
                        CreateDupResource(dupRes, filePath);
                        Dev2Logger.Debug(
                            string.Format(ErrorResource.ResourceAlreadyLoaded,
                                res.ResourceName, filePath, dupRes.FilePath), GlobalConstants.WarewolfDebug);
                    }
                    else
                    {
                        Dev2Logger.Debug(string.Format(
                                "Resource '{0}' from file '{1}' wasn't loaded because a resource with the same name has already been loaded but cannot find its location.",
                                res.ResourceName, filePath), GlobalConstants.WarewolfDebug);
                    }
                }
            }
        }

        void CreateDupResource(IResource resource, string filePath)
        {

            {
                var dupRes = _resources.Find(c => c.ResourceID == resource.ResourceID);
                if (dupRes != null)
                {
                    if (_duplicateResources.Any(p => p.ResourceId == dupRes.ResourceID))
                    {
                        var firstDup = _duplicateResources.First(p => p.ResourceId == dupRes.ResourceID);
                        if (!firstDup.ResourcePath.Contains(filePath))
                        {
                            firstDup.ResourcePath.Add(filePath);
                        }
                    }
                    var duplicatePaths = filePath == dupRes.FilePath ? string.Empty : filePath;
                    var resourcePaths = new List<string> { dupRes.FilePath };
                    if (!string.IsNullOrEmpty(duplicatePaths))
                    {
                        resourcePaths.Add(duplicatePaths);
                        var dupresource = new DuplicateResource
                        {
                            ResourceId = resource.ResourceID
                            ,
                            ResourceName = resource.ResourceName
                            ,
                            ResourcePath = resourcePaths
                        };
                        _duplicateResources.Add(dupresource);
                    }
                }
            }
        }
    }
}
