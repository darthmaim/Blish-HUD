﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {

    // TODO: The ColorPicker needs updates for events, it should probably inherit from FlowPanel,
    // and needs to get reconnected once we have Gorrik.NET included in the project
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorPicker : Panel {

        private const int COLOR_SIZE = 32;
        private const int COLOR_PADDING = 3;
        private const string BACKGROUND_TEXTURE_NAME = @"common\solid";

        public event EventHandler<EventArgs> SelectedColorChanged;

        public ObservableCollection<Gw2Sharp.WebApi.V2.Models.Color> Colors { get; protected set; }

        private readonly Dictionary<Gw2Sharp.WebApi.V2.Models.Color, ColorBox> colorBoxes;

        private int colorsPerRow;

        private Gw2Sharp.WebApi.V2.Models.Color selectedColor;
        public Gw2Sharp.WebApi.V2.Models.Color SelectedColor {
            get => selectedColor;
            protected set {
                if (selectedColor != value) {
                    selectedColor = value;
                    if (this.AssociatedColorBox != null) {
                        this.AssociatedColorBox.Color = value;
                    }
                    this.SelectedColorChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private ColorBox associatedColorBox;
        public ColorBox AssociatedColorBox {
            get => associatedColorBox;
            set {
                if (associatedColorBox != value) {
                    if (associatedColorBox != null)
                        associatedColorBox.Selected = false;

                    associatedColorBox = value;
                    associatedColorBox.Selected = true;
                    this.SelectedColor = this.AssociatedColorBox.Color;
                }
            }
        }

        public ColorPicker() : base() {
            this.Colors = new ObservableCollection<Gw2Sharp.WebApi.V2.Models.Color>();
            this.Colors.CollectionChanged += Colors_CollectionChanged;

            this.ContentRegion = new Rectangle(COLOR_PADDING, COLOR_PADDING, (this.Width - 10) - (COLOR_PADDING * 2), this.Height - (COLOR_PADDING * 2));

            colorBoxes = new Dictionary<Gw2Sharp.WebApi.V2.Models.Color, ColorBox>();
        }

        private void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // Remove any items that were removed (first because moving an item in a collection will put
            // that item in both OldItems and NewItems)
            if (e.OldItems != null) {
                foreach (var deletedItem in e
                                       .OldItems
                                       .Cast<Gw2Sharp.WebApi.V2.Models.Color>()
                                       .Where(delItem => colorBoxes.ContainsKey(delItem))) {
                    colorBoxes[deletedItem].Dispose();
                    colorBoxes.Remove(deletedItem);
                }
            }

            if (e.NewItems != null) {
                foreach (Gw2Sharp.WebApi.V2.Models.Color addedItem in e.NewItems) {
                    if (!colorBoxes.ContainsKey(addedItem)) {
                        var colorBox = new ColorBox() {
                            Color = addedItem,
                            Parent = this,
                            Size = new Point(COLOR_SIZE),
                        };

                        colorBoxes[addedItem] = colorBox;

                        colorBox.LeftMouseButtonPressed += delegate {
                            foreach (var box in colorBoxes.Values) {
                                box.Selected = false;
                            }

                            colorBox.Selected = true;
                            this.SelectedColor = colorBox.Color;
                        };
                    }
                }
            }

            // Relayout the color grid
            for (int i = 0; i < this.Colors.Count; i++) {
                var currentColor = this.Colors[i];
                var currentBox = colorBoxes[currentColor];

                int horizontalPosition = i % colorsPerRow;
                int verticalPosition = i / colorsPerRow;

                currentBox.Location = new Point(
                    horizontalPosition * (currentBox.Width + COLOR_PADDING),
                    verticalPosition * (currentBox.Width + COLOR_PADDING)
                );
            }
        }

        public override void Invalidate() {
            base.Invalidate();

            colorsPerRow = (this.Width -10 ) / (COLOR_SIZE + COLOR_PADDING);
            this.ContentRegion = new Rectangle(COLOR_PADDING, COLOR_PADDING, (this.Width - 10) - (COLOR_PADDING * 2), this.Height - (COLOR_PADDING * 2));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Draw background
            spriteBatch.DrawOnCtrl(this, Content.GetTexture(BACKGROUND_TEXTURE_NAME), bounds, Color.Black * 0.5f);
        }
    }
}
