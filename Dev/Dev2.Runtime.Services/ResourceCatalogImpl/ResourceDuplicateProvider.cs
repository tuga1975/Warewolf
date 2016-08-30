﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceDuplicateProvider : IResourceDuplicateProvider
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ResourceDuplicateProvider(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public ResourceCatalogResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences)
        {
            try
            {
                SaveFolders(sourcePath, destinationPath, newName, fixRefences);
                return ResourceCatalogResultBuilder.CreateSuccessResult("Duplicated Success");
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{sourcePath} ", x);
                return ResourceCatalogResultBuilder.CreateFailResult("Duplicated Failure " + x.Message);
            }
        }

        public ResourceCatalogResult DuplicateResource(Guid resourceId, string newPath, string newName)
        {

            try
            {
                SaveResource(resourceId, newName, newPath);
                return ResourceCatalogResultBuilder.CreateSuccessResult("Duplicated Success");
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{resourceId} ", x);
                return ResourceCatalogResultBuilder.CreateFailResult("Duplicated Failure " + x.Message);
            }
        }
        private void SaveResource(Guid resourceId, string newResourceName, string newPath = null)
        {

            StringBuilder result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            var xElement = result.ToXElement();

            resource.ResourcePath = newPath;
            resource.IsUpgraded = true;
            var resourceID = Guid.NewGuid();
            var resourceName = newResourceName.Split('\\').Last();
            newPath = string.IsNullOrEmpty(newPath) ? resource.ResourcePath : newPath;
            newPath = resource.ResourcePath == resource.FilePath && resource.ResourceName == resource.ResourcePath ? "" : newPath;
            resource.ResourcePath = newPath + "\\" + newResourceName;
            resource.ResourceName = resource.ResourceName != resourceName ? resourceName : resource.ResourceName;
            resource.ResourceID = resourceID;
            xElement.SetElementValue("DisplayName", resourceName);
            xElement.SetElementValue("ID", resourceID.ToString());
            xElement.SetElementValue("Category", resource.ResourcePath);
            var fixedResource = xElement.ToStringBuilder();
            _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource);

        }
        private void SaveFolders(string sourceLocation, string destination, string newName, bool fixRefences)
        {
            var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID);
            var resourceToMove = resourceList.Where(resource => resource.ResourcePath.ToUpper().StartsWith(sourceLocation.ToUpper()))
                                                .Where(resource => !(resource is ManagementServiceResource))
                                                .ToList();
            _resourcesToUpdate.AddRange(resourceToMove);
            foreach (var resource in resourceToMove)
            {
                try
                {
                    var newResourceName = destination + "\\" + newName;

                    var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                    var xElement = result.ToXElement();

                    resource.ResourcePath = newResourceName;
                    resource.IsUpgraded = true;
                    var newResourceId = Guid.NewGuid();
                    var oldResourceId = resource.ResourceID;
                    var resourceName = resource.ResourceName;
                    resource.ResourcePath = resource.ResourcePath;
                    resource.ResourceName = resource.ResourceName;
                    resource.ResourceID = newResourceId;
                    xElement.SetElementValue("DisplayName", resourceName);
                    xElement.SetElementValue("ID", newResourceId.ToString());
                    xElement.SetElementValue("Category", resource.ResourcePath);
                    var fixedResource = xElement.ToStringBuilder();
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource);
                    _resourceUpdateMap.Add(new KeyValuePair<Guid, Guid>(newResourceId, oldResourceId));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _resourcesToUpdate.Remove(resource);
                }
            }
            if (fixRefences)
            {

                try
                {
                    using (var tx = new System.Transactions.TransactionScope())
                    {
                        FixReferences();
                        tx.Complete();
                    }

                }
                catch (Exception e)
                {
                    Transaction.Current.Rollback();
                    throw new Exception("Failure Fixing references", e);
                }
            }
        }

        private readonly List<IResource> _resourcesToUpdate = new List<IResource>();
        private readonly List<KeyValuePair<Guid, Guid>> _resourceUpdateMap = new List<KeyValuePair<Guid, Guid>>();
        private void FixReferences()
        {
            foreach (var keyValuePair in _resourceUpdateMap)
            {
                var resourceToFix = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, keyValuePair.Value);
                var contents = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, keyValuePair.Value);
                //var resourceToFix = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, keyValuePair.Value);
                if (resourceToFix == null)
                {
                    ResourceCatalog.Instance.Reload();
                    resourceToFix = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, keyValuePair.Value);
                }

                var resourceForTrees = resourceToFix.Dependencies;
                foreach (var resourceForTree in resourceForTrees)
                {
                    var oldRefences = _resourceUpdateMap.Where(pair => pair.Value == resourceForTree.ResourceID);
                    foreach (KeyValuePair<Guid, Guid> oldRefence in oldRefences)
                    {
                        resourceForTree.ResourceID = oldRefence.Value;//assign new ID 
                        contents = contents.ToString().Replace(resourceForTree.ResourceID.ToString(), oldRefence.Value.ToString()).ToStringBuilder();
                    }
                }
                resourceToFix.Dependencies = resourceForTrees;
                if (resourceToFix.Dependencies.Any())
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resourceToFix, contents);
            }
        }
    }
}