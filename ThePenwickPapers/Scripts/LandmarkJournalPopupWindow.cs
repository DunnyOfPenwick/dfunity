// Project:   Landmark Journal for Daggerfall Unity
// Author:    DunnyOfPenwick
// Origin Date: Apr 2021

using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace ThePenwickPapers
{
    public class LandmarkJournalPopupWindow : DaggerfallPopupWindow
    {
        private Rect travelButtonRect = new Rect(5, 5, 120, 7);
        private Rect rememberButtonRect = new Rect(5, 14, 120, 7);
        private Rect forgetButtonRect = new Rect(5, 23, 120, 7);
        private Rect exitButtonRect = new Rect(44, 32, 43, 10);

        private List<LandmarkLocation> locations = null;

        protected Panel mainPanel = new Panel();
        protected Button travelButton = new Button();
        protected Button rememberButton = new Button();
        protected Button forgetButton = new Button();
        protected Button exitButton = new Button();


        public LandmarkJournalPopupWindow(IUserInterfaceManager uiManager, List<LandmarkLocation> locations)
            : base(uiManager)
        {
            ParentPanel.BackgroundColor = Color.clear;

            this.locations = locations;
        }


        protected override void Setup()
        {
            // Create interface panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.Position = new Vector2(0, 50);
            mainPanel.Size = new Vector2(130, 62);
            DaggerfallUI.Instance.SetDaggerfallPopupStyle(DaggerfallUI.PopupStyle.Parchment, mainPanel);


            bool hasRememberedLocationNearby = IsRememberedLocationNearby();
            Color32 disabledColor = new Color32(100, 100, 100, 255);

            // "Travel to..." button
            travelButton = DaggerfallUI.AddButton(travelButtonRect, mainPanel);
            travelButton.HorizontalAlignment = HorizontalAlignment.Center;
            travelButton.Label.Text = Text.TravelButtonText.Get();
            if (locations.Count == 0)
            {
                travelButton.Label.TextColor = disabledColor;
            }
            else
            {
                travelButton.OnMouseClick += TravelButton_OnMouseClick;
            }


            // "Remember this spot" button
            rememberButton = DaggerfallUI.AddButton(rememberButtonRect, mainPanel);
            rememberButton.HorizontalAlignment = HorizontalAlignment.Center;
            rememberButton.Label.Text = Text.RememberButtonText.Get();
            if (hasRememberedLocationNearby)
            {
                rememberButton.Label.TextColor = disabledColor;
            }
            else
            {
                rememberButton.OnMouseClick += RememberButton_OnMouseClick;
            }

            // "Forget this spot" button
            forgetButton = DaggerfallUI.AddButton(forgetButtonRect, mainPanel);
            forgetButton.HorizontalAlignment = HorizontalAlignment.Center;
            forgetButton.Label.Text = Text.ForgetButtonText.Get();
            if (hasRememberedLocationNearby)
            {
                forgetButton.OnMouseClick += ForgetButton_OnMouseClick;
            }
            else
            {
                forgetButton.Label.TextColor = disabledColor;
            }

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            exitButton.HorizontalAlignment = HorizontalAlignment.Center;
            exitButton.OnMouseClick += ExitButton_OnMouseClick;
            exitButton.Label.Text = Text.ExitButtonText.Get();


            NativePanel.Components.Add(mainPanel);
        }


        protected virtual void TravelButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);

            DaggerfallUI.Instance.UserInterfaceManager.PopWindow();

            LandmarkJournalListPickerWindow locationPicker = new LandmarkJournalListPickerWindow(locations);

            DaggerfallUI.Instance.UserInterfaceManager.PushWindow(locationPicker);
        }



        /// <summary>
        /// Checks if a remembered location is nearby.  Used to enable the 'Forget' button.
        /// </summary>
        private bool IsRememberedLocationNearby()
        {
            foreach (LandmarkLocation loc in locations)
            {
                if (GameManager.Instance.PlayerMotor.DistanceToPlayer(loc.Position) < 5)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Triggered when the 'Remember this spot..." button is clicked.
        /// Shows a text input messagebox to allow user to enter a label.
        /// </summary>
        protected virtual void RememberButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();

            DaggerfallInputMessageBox inputMessageBox = new DaggerfallInputMessageBox(DaggerfallUI.UIManager, this);
            inputMessageBox.SetTextBoxLabel(Text.LandmarkName.Get());
            inputMessageBox.InputDistanceX = 10;
            inputMessageBox.OnGotUserInput += InputMessageBox_OnGotUserInput;
            inputMessageBox.Show();
        }


        /// <summary>
        /// Gets the resulting input from the text input messagebox and records the current location.
        /// </summary>
        protected virtual void InputMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            string name = input.Trim();
            if (name.Length > 0)
            {
                Vector3 currentPosition = GameManager.Instance.PlayerMotor.transform.position;
                locations.Add(new LandmarkLocation(name, currentPosition));
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
            }

        }


        /// <summary>
        /// Triggered when the 'Forget this spot..." button is clicked.
        /// Removes landmark corresponding to current location.
        /// </summary>
        protected virtual void ForgetButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ParchmentScratching);
            CloseWindow();

            locations.RemoveAll(loc => GameManager.Instance.PlayerMotor.DistanceToPlayer(loc.Position) < 5);
        }


        /// <summary>
        /// Triggered when the 'Exit" button is clicked.
        /// Closes the dialog window.
        /// </summary>
        protected virtual void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
        }


    }

}
