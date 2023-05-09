using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MCNBTEditor.Core;

namespace MCNBTEditor.Resources {
    /// <summary>
    /// A converter for converting icon types into their underlying URI
    /// </summary>
    [ValueConversion(typeof(IconType), typeof(Uri))]
    public class IconTypeToUriConverter : IValueConverter {
        public static IconTypeToUriConverter Instance { get; } = new IconTypeToUriConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            if (!(value is IconType type))
                throw new Exception($"Expected {nameof(IconType)}, got {value}");

            Uri uri = IconTypeToUri(type);
            return uri != null ? uri : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private static string GetResourcePath(string fileInResources) {
            return $"/MCNBTEditor;component/Resources/{fileInResources}";
        }

        private static Uri GetUri(string fileInResources) {
            return new Uri(GetResourcePath(fileInResources), UriKind.Relative);
        }

        public static Uri IconTypeToUri(IconType type) {
            switch (type) {
                case IconType.ITEM_TAG_End:                 return GetUri("Icons/FileIcon-TagEnd.png");
                case IconType.ITEM_TAG_Byte:                return GetUri("Icons/FileIcon-TagByte8.png");
                case IconType.ITEM_TAG_Short:               return GetUri("Icons/FileIcon-TagShort16.png");
                case IconType.ITEM_TAG_Int:                 return GetUri("Icons/FileIcon-TagInt32.png");
                case IconType.ITEM_TAG_Long:                return GetUri("Icons/FileIcon-TagLong64.png");
                case IconType.ITEM_TAG_Float:               return GetUri("Icons/FileIcon-TagFloat328.png");
                case IconType.ITEM_TAG_Double:              return GetUri("Icons/FileIcon-TagDouble64.png");
                case IconType.ITEM_TAG_String:              return GetUri("Icons/FileIcon-TagString.png");
                case IconType.ITEM_TAG_ByteArray:           return GetUri("Icons/FileIcon-TagByteArray.png");
                case IconType.ITEM_TAG_IntArray:            return GetUri("Icons/FileIcon-TagIntArray.png");
                case IconType.ITEM_TAG_LongArray:           return GetUri("Icons/FileIcon-TagLongArray.png");
                case IconType.ITEM_TAG_List:                return GetUri("Icons/icons8-bulleted-list-48.png");
                case IconType.ITEM_TAG_Compound_Closed:     return GetUri("Icons/icons8-closed-box-48.png");
                case IconType.ITEM_TAG_Compound_OpenFull:   return GetUri("Icons/icons8-open-box-48.png");
                case IconType.ITEM_TAG_Compound_OpenEmpty:  return GetUri("Icons/icons8-empty-box-48.png");
                case IconType.ITEM_Refresh:                 return GetUri("Icons/icons8-sync-48.png");
                case IconType.ITEM_DATFile:                 return GetUri("Icons/icons8-closed-box-48.png");
                case IconType.ITEM_RegionFile:              return GetUri("Icons/FileIcon-Region.png");
                case IconType.ACTION_TAG_CopyName:          return GetUri("Icons/icons8-copy-to-clipboard-48.png");
                case IconType.ACTION_TAG_CopyValue:         return null;// return new Uri("/Icons/FileIcon-TagByte8.png");
                case IconType.ACTION_TAG_CopyBinary:        return GetUri("Icons/icons8-copy-48.png");
                case IconType.ACTION_TAG_PasteBinary:       return GetUri("Icons/icons8-paste-48.png");
                case IconType.ACTION_TAG_Delete:            return null;// return new Uri("/Icons/FileIcon-TagByte8.png");
                case IconType.ACTION_TAG_Rename:            return null;// return new Uri("/Icons/FileIcon-TagByte8.png");
                case IconType.ACTION_TAG_EditGeneral:       return GetUri("Icons/icons8-edit-48.png");
                default: return null;
            }
        }
    }
}
