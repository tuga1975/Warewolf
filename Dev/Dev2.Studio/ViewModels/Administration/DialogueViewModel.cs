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
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dev2.Common.Interfaces.Studio;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.AppResources;


namespace Dev2.Studio.ViewModels.Administration
{

    public class DialogueViewModel : IDialogueViewModel
    {

        #region Members

        readonly ClosedOperationEventHandler OnOkClick;
        ICommand _okClicked;
        ICommand _hyperLink;
        ImageSource _imageSource;
        string _description;
        string _title;
        string _descriptionTitleText;

        #endregion Members

        #region Properties

        public String Title => _title;

        public ImageSource ImageSource => _imageSource;


        public string DescriptionTitleText => _descriptionTitleText;

        public String DescriptionText => _description;

        public string Hyperlink { get; private set; }
        public string HyperlinkText { get; private set; }
        public Visibility HyperlinkVisibility { get; private set; }

        public ICommand HyperLinkCommand => _hyperLink ?? (_hyperLink = new RelayCommand(p => Hyperlink_OnMouseDown()));

        public ICommand OkCommand => _okClicked ?? (_okClicked = new RelayCommand(p =>
                                                   {
                                                       OnOkClick?.Invoke(this, null);
                                                   }, p => true));

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Show the EULA page ;)
        /// </summary>
        public void Hyperlink_OnMouseDown()
        {
            Process.Start(new Uri(Hyperlink).AbsoluteUri);
        }

        public void SetupDialogue(string title, string description, string imageSourceuri, string descriptionTitleText) => SetupDialogue(title, description, imageSourceuri, descriptionTitleText, null, null);

        public void SetupDialogue(string title, string description, string imageSourceuri, string descriptionTitleText, string hyperlink, string linkText)
        {
            SetTitle(title);
            SetDescription(description);
            SetImage(imageSourceuri);
            SetDescriptionTitleText(descriptionTitleText);
            SetHyperlink(hyperlink, linkText);
        }

        #endregion

        #region Private Methods

        void SetTitle(string title)
        {
            _title = string.IsNullOrEmpty(title) ? string.Empty : title;
        }

        void SetDescription(string description)
        {
            _description = string.IsNullOrEmpty(description) ? string.Empty : description;
        }

        void SetImage(string imageSource)
        {
            if (string.IsNullOrEmpty(imageSource))
            {
                _imageSource = null;
            }
            else
            {
                var validUri = Uri.TryCreate(imageSource, UriKind.RelativeOrAbsolute, out Uri imageUri);

                if (validUri)
                {

                    // Once initialized, the image must be released so that it is usable by other resources
                    var btMap = new BitmapImage();
                    btMap.BeginInit();
                    btMap.UriSource = imageUri;
                    btMap.CacheOption = BitmapCacheOption.OnLoad;
                    btMap.EndInit();
                    _imageSource = btMap;
                }
                else
                {
                    throw new UriFormatException(String.Format("Uri :{0} was not in the correct format", imageSource));
                }
            }

        }



        void SetDescriptionTitleText(string text)
        {
            _descriptionTitleText = string.IsNullOrEmpty(text) ? string.Empty : text;
        }

        void SetHyperlink(string link, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Hyperlink = link;
                HyperlinkText = text;
                HyperlinkVisibility = Visibility.Visible;
            }

            HyperlinkVisibility = Visibility.Collapsed;
        }

        #endregion Private Methods

        #region IDisposable Implementaton

        public void Dispose()
        {
            _imageSource = null;
            _description = null;
            _descriptionTitleText = null;
            _title = null;
        }

        #endregion IDisposable Implementation

    }
}
