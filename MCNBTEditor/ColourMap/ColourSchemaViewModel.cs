using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MCNBTEditor.ColourMap.Maps;
using MCNBTEditor.Core.Utils;

namespace MCNBTEditor.ColourMap {
    /// <summary>
    /// A schema stores all of the colour maps, and is usually applied for the entire app
    /// </summary>
    public class ColourSchemaViewModel {
        private readonly Dictionary<string, BaseMapItemViewModel> idToItem;

        private readonly ObservableCollectionEx<ColourMapViewModel> colourMaps;
        public ReadOnlyObservableCollection<ColourMapViewModel> ColourMaps { get; }

        public IEnumerable<BrushItemViewModel> DirtyBrushes => this.idToItem.Values.OfType<BrushItemViewModel>().Where(x => x.HasBeenModified);

        public ColourSchemaViewModel() {
            this.idToItem = new Dictionary<string, BaseMapItemViewModel>();
            this.colourMaps = new ObservableCollectionEx<ColourMapViewModel>();
            this.ColourMaps = new ReadOnlyObservableCollection<ColourMapViewModel>(this.colourMaps);
        }

        public static ColourSchemaViewModel CreateDarkMode() {
            ColourSchemaViewModel schema = new ColourSchemaViewModel();
            ColourMapViewModel generalMap = schema.CreateColourMap("General");
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone1.Background", "Container background 1"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone1.BorderBrush", "Container border 1"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone2.Background", "Container background 2"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone2.BorderBrush", "Container border 2"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone3.Background", "Container background 3"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone3.BorderBrush", "Container border 3"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone4.Background", "Container background 4"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone4.BorderBrush", "Container border 4"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone5.Background", "Container background 5"));
            generalMap.AddItem(new BrushItemViewModel(schema, generalMap, "Container.Tone5.BorderBrush", "Container border 5"));
            ColourMapViewModel ooo = schema.CreateColourMap("Yoinks");
            ooo.AddItem(new BrushItemViewModel(schema, ooo, "Nasty.no1.Background", "Nasty background 1"));
            ooo.AddItem(new BrushItemViewModel(schema, ooo, "Nasty.no1.BorderBrush", "Nasty border 2"));
            ooo.AddItem(new BrushItemViewModel(schema, ooo, "Nasty.no2.Background", "Nasty background 3"));
            ooo.AddItem(new BrushItemViewModel(schema, ooo, "Nasty.no2.BorderBrush", "Nasty border 4"));
            schema.BuildMapCache();
            return schema;
        }

        public void BuildMapCache() {
            this.idToItem.Clear();
            foreach (ColourMapViewModel map in this.colourMaps) {
                this.BuildMapCacheRecursive(map);
            }
        }

        private void BuildMapCacheRecursive(BaseMapItemViewModel item) {
            if (item is ColourMapViewModel colourMap) {
                foreach (BaseMapItemViewModel innerItem in colourMap.items) {
                    this.BuildMapCacheRecursive(innerItem);
                }
            }
            else if (item is BrushItemViewModel brush) {
                if (this.idToItem.TryGetValue(brush.Id, out BaseMapItemViewModel existing) && existing != null) {
                    throw new Exception($"Item already exists with the ID '{brush.Id}': {existing.FullName}");
                }

                this.idToItem[brush.Id] = item;
            }
        }

        public bool TryGetItemById(string id, out BaseMapItemViewModel value) {
            return this.idToItem.TryGetValue(id, out value);
        }

        public BaseMapItemViewModel GetItemById(string id) {
            return this.TryGetItemById(id, out BaseMapItemViewModel item) ? item : null;
        }

        public ColourMapViewModel CreateColourMap(string name) {
            ColourMapViewModel map = new ColourMapViewModel(this, null, name);
            this.colourMaps.Add(map);
            return map;
        }
    }
}