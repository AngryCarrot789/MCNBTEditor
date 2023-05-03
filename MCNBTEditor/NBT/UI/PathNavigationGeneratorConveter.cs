using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using MCNBTEditor.Core.Explorer;
using MCNBTEditor.Views.Main;

namespace MCNBTEditor.NBT.UI {
    public class PathNavigationGeneratorConveter : IValueConverter {
        public Style SeparatorRunStyle { get; set; }
        public Style HyperlinkRunStyle { get; set; }
        public Style HyperlinkStyle { get; set; }

        public bool AcceptItemForPath { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is List<string> list)) {
                if (this.AcceptItemForPath && value is BaseTreeItemViewModel item) {
                    list = item.GetPathChain().ToList();
                }
                else if (value != DependencyProperty.UnsetValue) {
                    return new List<Inline>() {
                        this.CreateSeparator("<root>")
                    };
                }
                else {
                    return DependencyProperty.UnsetValue;
                }
            }

            List<Inline> inlines = new List<Inline>();
            using (List<string>.Enumerator enumerator = list.GetEnumerator()) {
                StringBuilder sb = new StringBuilder();
                if (enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current)) {
                    inlines.Add(this.CreateHyperlink(enumerator.Current, sb.Append(enumerator.Current)));
                }

                while (enumerator.MoveNext()) {
                    inlines.Add(this.CreateSeparator("/"));
                    inlines.Add(this.CreateHyperlink(enumerator.Current, sb.Append('/').Append(enumerator.Current)));
                }
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private Run CreateSeparator(string text) {
            return this.SeparatorRunStyle != null ? new Run(text) {Style = this.SeparatorRunStyle} : new Run(text);
        }

        private Hyperlink CreateHyperlink(string text, StringBuilder accumulatedFullPath) {
            Run run = this.HyperlinkRunStyle != null ? new Run(text) {Style = this.HyperlinkRunStyle} : new Run(text);
            Hyperlink link = this.HyperlinkStyle != null ? new Hyperlink(run) {Style = this.HyperlinkStyle} : new Hyperlink(run);
            link.Tag = accumulatedFullPath.ToString();
            link.Click += OnLinkClicked;
            return link;
        }

        private static async void OnLinkClicked(object sender, RoutedEventArgs e) {
            if (sender is Hyperlink link && link.Tag is string path) {
                if (link.DataContext is MainViewModel mvm) {
                    await mvm.NavigateToPath(path);
                }
                else if (Window.GetWindow(link) is MainWindow mWin && mWin.DataContext is MainViewModel mvm2) {
                    await mvm2.NavigateToPath(path);
                }
            }
        }
    }
}