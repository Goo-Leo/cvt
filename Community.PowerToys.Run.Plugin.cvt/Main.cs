using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Windows.UI;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.cvt
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "2F9FD69EAE55433D84BE40C775C5ACE3";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "cvt";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Type your data";

        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        { 
            string input = query.Search;
            var results = new List<Result>();

            if (int.TryParse(input, out int number))
            {
                // Convert number to binary
                string binary = Convert.ToString(number, 2);
                string hex = Convert.ToString(number, 16);
                results.Add(new Result
                {
                    Title = $"Decimal to Binary: {binary}",
                    SubTitle = $"Input: {number}",
                    Action = _ =>
                    {
                        Clipboard.SetText(binary);
                        return true;
                    }
                });
                results.Add(new Result
                {
                    Title = $"Decimal to Hex(Big Endian): 0x{hex}",
                    SubTitle = $"Input: {number}",
                    Action = _ =>
                    {
                        Clipboard.SetText("0x"+hex);
                        return true;
                    }
                });
                results.Add(new Result
                {
                    Title = $"Decimal to Hex(Little Endian): 0x{FlipEndian(hex)}",
                    SubTitle = $"Input: {number}",
                    Action = _ =>
                    {
                        Clipboard.SetText("0x"+FlipEndian(hex));
                        return true;
                    }
                });
            } else if (IsHexString(input)) { 
                int HexNumber = Convert.ToInt32(input[2..], 16);
                results.Add(new Result
                {
                    Title = $"Hex to Binary: {Convert.ToString(HexNumber,2)}",
                    SubTitle = $"Input: {input}",
                    Action = _ =>
                    {
                        Clipboard.SetText(HexNumber.ToString());
                        return true;
                    }
                });
                string flipEndian = FlipEndian(input);
                results.Add(new Result
                {
                    Title = $"Hex to Decimal: {HexNumber}",
                    SubTitle = $"Input: {input}",
                    Action = _ =>
                    {
                        Clipboard.SetText(HexNumber.ToString());
                        return true;
                    }
                });
                results.Add(new Result {
                    Title = $"Flip Endian: {flipEndian}",
                    SubTitle = $"Input: {input}",
                    Action = _ =>
                    {
                        Clipboard.SetText(flipEndian);
                        return true;
                    }
                });
            }
            else
            {
                results.Add(new Result
                {
                    Title = "Invalid Input",
                    SubTitle = "Enter a Decimal or Hex number!",
                });
            }

            return results;
        }

        private static bool IsHexString(string input)
        {
            if (!(input.StartsWith("0x")&&input.Length<=10)) { 
                return false;
            }
            return true;
        }
        private static string FlipEndian(string hex)
        {
            bool hasPrefix = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            if (hasPrefix){
                hex = hex.Substring(2);
            }
            if (hex.Length % 2 != 0){
                hex = "0" + hex;
            }

            var bytes = Enumerable.Range(0, hex.Length / 2)
                .Select(i => hex.Substring(i * 2, 2))
                .ToArray();
            Array.Reverse(bytes);
            string result = string.Join("", bytes);

            return hasPrefix ? "0x" + result : result;
        }


        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string search)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy to clipboard (Ctrl+C)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            Clipboard.SetDataObject(search);
                            return true;
                        },
                    }
                ];
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/cvt.light.png" : "Images/cvt.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
