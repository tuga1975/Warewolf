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

namespace Dev2.Activities.Designers2.Core
{
    public class IsItemDragged
    {

        #region Fields

        static readonly Lazy<IsItemDragged> _instance = new Lazy<IsItemDragged>(() => new IsItemDragged());


        bool _isDragged;

        #endregion

        #region Ctor

        IsItemDragged()
        {
            IsDragged = false;
        }

        #endregion

        #region Properties

        public static IsItemDragged Instance => _instance.Value;

        
        public bool IsDragged
        {
            get
            {
                return _isDragged;
            }
            set
            {
                _isDragged = value;
            }
        }

        #endregion
    }
}
