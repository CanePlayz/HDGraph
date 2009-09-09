﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.ComponentModel;

namespace HDGraph.Interfaces.DrawEngines
{
    public enum ModeAffichageCouleurs
    {
        RandomNeutral,
        RandomBright,
        Linear,
        Linear2,
        ImprovedLinear
    }

    public class DrawOptions : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private Font textFont;
        public Font TextFont
        {
            get { return textFont; }
            set
            {
                if (textFont != value)
                {
                    textFont = value;
                    RaisePropertyChanged("TextFont");
                }
            }
        }

        private bool showSize;
        public bool ShowSize
        {
            get { return showSize; }
            set
            {
                if (showSize != value)
                {
                    showSize = value;
                    RaisePropertyChanged("ShowSize");
                }
            }
        }

        private int shownLevelsCount;
        public int ShownLevelsCount 
        {
            get { return shownLevelsCount; }
            set
            {
                if (shownLevelsCount != value)
                {
                    shownLevelsCount = value;
                    RaisePropertyChanged("ShownLevelsCount");
                }
            }
        }

        private ModeAffichageCouleurs colorStyleChoice;
        public ModeAffichageCouleurs ColorStyleChoice
        {
            get { return colorStyleChoice; }
            set
            {
                if (colorStyleChoice != value)
                {
                    colorStyleChoice = value;
                    RaisePropertyChanged("ColorStyleChoice");
                }
            }
        }

        private int imageRotation;
        public int ImageRotation
        {
            get { return imageRotation; }
            set
            {
                if (imageRotation != value)
                {
                    imageRotation = value;
                    RaisePropertyChanged("ImageRotation");
                }
            }
        }

        private int textDensity;
        /// <summary>
        /// Angle min to enable text print.
        /// </summary>
        public int TextDensity
        {
            get { return textDensity; }
            set
            {
                if (textDensity != value)
                {
                    textDensity = value;
                    RaisePropertyChanged("TextDensity");
                }
            }
        }


        public virtual DrawOptions Clone()
        {
            return (DrawOptions)this.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null
                || !(obj is DrawOptions))
                return false;
            if ((object)this == obj)
                return true;
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.GetValue(this, null).Equals(property.GetValue(obj, null)))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
