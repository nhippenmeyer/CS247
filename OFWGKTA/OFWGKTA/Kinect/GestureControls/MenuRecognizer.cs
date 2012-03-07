using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Microsoft.Research.Kinect.Nui;
using System.Timers;

namespace OFWGKTA
{
    public class MenuRecognizer : ViewModelBase, IGestureRecognizer
    {
        private double selectionZTolerance = 0.15;

        private Timer selectionTimer;
        private bool menuEnabled = false;
        private bool isHorizontal; // whether menu is enabled above head and panned L/R, or to right and U/D
        private int maxIndex;
        private int menuSize; // width (isHorizontal = true) or height (isHorizontal = false) of menu
        private Vector center; // center assigned on menuEnabled
        private int hoverIndex = -1; // index of item hovered over from 0 to numberOfItems - 1
        private int selectedIndex = -1;
        public bool selectionDead = false;
        private bool isClutched = false;

        public event EventHandler<MenuEventArgs> MenuItemSelected;

        public bool Disabled { get; private set; }

        public MenuRecognizer(int numberOfItems, int menuSize)
            : this(numberOfItems, menuSize, true) { }

        public MenuRecognizer(int numberOfItems, int menuSize, bool isHorizontal)
        {
            this.Disabled = false;
            this.isHorizontal = isHorizontal;
            this.maxIndex = numberOfItems - 1;
            this.menuSize = menuSize;
        }

        #region Timer
        private void SelectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.SelectionDead = true;
            this.MenuEnabled = false;
            if (this.MenuItemSelected != null)
            {
                this.MenuItemSelected(this, new MenuEventArgs()
                {
                    SelectedIndex = this.SelectedIndex
                });
            }
        }

        private void StartTimer(double seconds)
        {
            this.selectionTimer = new Timer(seconds * 1000);
            this.selectionTimer.AutoReset = false;
            this.selectionTimer.Elapsed += new ElapsedEventHandler(SelectionTimer_Elapsed);
            this.selectionTimer.Enabled = true;
        }

        private void StopTimer()
        {
            if (this.selectionTimer != null)
            {
                this.selectionTimer.Dispose();
            }
        }
        #endregion

        #region Menu
        private void ShowMenu(Vector center)
        {
            this.MenuEnabled = true;
            this.center = center;
            this.IsClutched = true;
        }

        private void HideMenu()
        {
            this.MenuEnabled = false;
            this.HoverIndex = -1;
            this.SelectedIndex = -1;
            this.IsClutched = false;
        }

        public void Disable()
        {
            this.Disabled = true;
            HideMenu();
            StopTimer();
            this.SelectionDead = false;
        }

        public void Enable()
        {
            this.Disabled = false;
        }
        #endregion

        #region ProcessDeltas
        private void ProcessDeltaZ(float deltaZ)
        {
            if (deltaZ > this.selectionZTolerance)
            {
                if (this.SelectedIndex < 0)
                {
                    this.SelectedIndex = this.HoverIndex;
                    StartTimer(1.5);
                }
            }
            else
            {
                StopTimer();
                this.SelectedIndex = -1;
            }
        }

        private void ProcessDelta(float delta)
        {
            this.HoverIndex = Math.Min(this.maxIndex, Math.Max(0, (int)Math.Ceiling((delta / this.menuSize + 0.5) * this.maxIndex)));
        }
        #endregion

        #region AddPoints
        private void AddHorizontal(Vector handRight, Vector shoulderCenter, Vector shoulderRight)
        {
            if (handRight.Y < shoulderCenter.Y)
            {
                if (!this.SelectionDead)
                {
                    if (!this.MenuEnabled)
                    {
                        ShowMenu(handRight);
                    }
                    ProcessDelta(handRight.X - center.X);
                    ProcessDeltaZ(center.Z - handRight.Z);
                }
            }
            else
            {
                StopTimer();
                this.SelectionDead = false;
                HideMenu();
            }
        }

        private void AddVertical(Vector handRight, Vector shoulderCenter, Vector shoulderRight)
        {
            float shoulderWidth = (shoulderRight.X - shoulderCenter.X) * 2;
            if (handRight.X - shoulderRight.X > shoulderWidth)
            {
                if (!this.SelectionDead)
                {
                    if (!this.MenuEnabled)
                    {
                        ShowMenu(handRight);
                    }
                    ProcessDelta(handRight.Y - center.Y);
                    ProcessDeltaZ(center.Z - handRight.Z);
                }
            }
            else
            {
                StopTimer();
                this.SelectionDead = false;
                HideMenu();
            }
        }

        public void Add(KinectModel kinect)
        {
            if (!Disabled)
            {
                if (isHorizontal)
                {
                    AddHorizontal(kinect.HandRight, kinect.ShoulderCenter, kinect.ShoulderRight);
                }
                else
                {
                    AddVertical(kinect.HandRight, kinect.ShoulderCenter, kinect.ShoulderRight);
                }
            }
        }
        #endregion

        #region Properties
        public bool MenuEnabled
        {
            get { return this.menuEnabled; }
            private set
            {
                if (this.menuEnabled != value)
                {
                    this.menuEnabled = value;
                    RaisePropertyChanged("MenuEnabled");
                }
            }
        }

        public int HoverIndex
        {
            get { return this.hoverIndex; }
            private set
            {
                if (this.hoverIndex != value)
                {
                    this.hoverIndex = value;
                    RaisePropertyChanged("HoverIndex");
                }
            }
        }

        public bool SelectionDead
        {
            get { return this.selectionDead; }
            private set
            {
                if (this.selectionDead != value)
                {
                    this.selectionDead = value;
                    RaisePropertyChanged("SelectionDead");
                }
            }
        }

        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            private set
            {
                if (this.selectedIndex != value)
                {
                    this.selectedIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }

        public bool IsClutched
        {
            get { return this.isClutched; }
            private set
            {
                if (this.isClutched != value)
                {
                    this.isClutched = value;
                    RaisePropertyChanged("IsClutched");
                }
            }
        }
        #endregion
    }

    public class MenuEventArgs : EventArgs
    {
        public int SelectedIndex;
    }

}
