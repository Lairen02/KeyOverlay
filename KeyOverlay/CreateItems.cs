using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SFML.Graphics;
using SFML.System;

namespace KeyOverlay
{
    public static class CreateItems
    {
        private static readonly Font _font = new Font(Path.GetFullPath(Path.Combine
        (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Resources",
            "consolab.ttf")));
        public static RectangleShape CreateBar(RectangleShape square, int outlineThickness, float barSpeed)
        {
            var rect = new RectangleShape(new Vector2f(((AppWindow.instance.defaultKeySize + outlineThickness * 2) * AppWindow.instance.barScaleMult)+1, barSpeed));
            rect.Position = new Vector2f(square.Position.X  - outlineThickness - AppWindow.instance.sizeSteps/2f - ((rect.Size.X - (AppWindow.instance.defaultKeySize + outlineThickness * 2))/2f),
                square.Position.Y - AppWindow.instance.defaultKeySize - outlineThickness - AppWindow.instance.sizeSteps/2f - AppWindow.instance.barOffsetY);
            rect.FillColor = square.FillColor;
            return rect;
        }

        public static List<RectangleShape> CreateKeys(int keyAmount, int outlineThickness, float size, float ratioX, float ratioY,
            int margin, RenderWindow window, Color backgroundColor, Color outlineColor, int layoutKeyAmount = 0)
        {
            var yPos = 900 * ratioY;
            var width = size + outlineThickness * 2;
            var keyList = new List<RectangleShape>();
            var spacingKeyAmount = Math.Max(keyAmount, layoutKeyAmount);
            var spacing = spacingKeyAmount <= 1
                ? 0
                : (window.Size.X - margin * 2 - width * spacingKeyAmount) / (spacingKeyAmount - 1);
            var totalWidth = width * keyAmount + spacing * Math.Max(0, keyAmount - 1);
            var startX = (window.Size.X - totalWidth) / 2f + outlineThickness;

            for (int i = 0; i < keyAmount; i++)
            {
                var square = new RectangleShape(new Vector2f(size, size));
                
                square.FillColor = backgroundColor;
                square.OutlineColor = outlineColor;
                square.OutlineThickness = outlineThickness;
                square.Origin = new Vector2f(0, size);
                square.Position = new Vector2f(startX + (width + spacing) * i, yPos);
                keyList.Add(square);
            }
            return keyList;
        }

        public static Text CreateText(string key, RectangleShape square, Color color, bool counter)
        {
            var text = new Text(key, _font);
            text.CharacterSize = (uint)(50 * square.Size.X / 140);
            text.Style = Text.Styles.Bold;
            text.FillColor = color;
            text.Origin = new Vector2f(text.GetLocalBounds().Width / 2f, 32 * square.Size.X / 140f);
            if(counter)
                text.Position = new Vector2f(square.GetGlobalBounds().Left + square.OutlineThickness + square.Size.X / 2f,
                    square.GetGlobalBounds().Top + square.OutlineThickness + square.Size.Y +text.CharacterSize);
            else
                text.Position = new Vector2f(square.GetGlobalBounds().Left + square.OutlineThickness + square.Size.X / 2f,
                    square.GetGlobalBounds().Top + square.OutlineThickness + square.Size.Y / 2f);

            return text;
        }
        
        public static Color CreateColor(string s)
        {
            var bytes = s.Split(',').Select(int.Parse).Select(Convert.ToByte).ToArray();
            return new Color(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
    }
}
