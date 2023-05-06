using System;
using System.Collections.Generic;
using System.Linq;

namespace MCNBTEditor.Core.Actions.Contexts {
    public class DataContext : IDataContext {
        public Dictionary<string, object> CustomDataMap { get; set; }
        public List<object> ContextList { get; }

        public IEnumerable<object> Context => this.ContextList;

        public IEnumerable<(string, object)> CustomData => this.CustomDataMap != null ? this.CustomDataMap.Select(x => (x.Key, x.Value)) : Enumerable.Empty<(string, object)>();

        public DataContext() {
            this.ContextList = new List<object>();
        }

        public DataContext(object primaryContext) : this() {
            this.AddContext(primaryContext);
        }

        public void AddContext(object context) {
            this.ContextList.Add(context);
        }

        public T GetContext<T>() {
            this.TryGetContext(out T value); // value will be default or null
            return value;
        }

        public bool TryGetContext<T>(out T value) {
            foreach (object obj in this.ContextList) {
                if (obj is T t) {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool HasContext<T>() {
            return this.ContextList.Any(x => x is T);
        }

        public bool TryGet<T>(string key, out T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (this.CustomDataMap != null && this.CustomDataMap.TryGetValue(key, out object data) && data is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(string key) {
            return this.TryGet<object>(key, out _);
        }

        public bool HasFlag(string key) {
            return this.TryGet(key, out bool value) && value;
        }

        public T Get<T>(string key) {
            this.TryGet(key, out T value);
            return value; // ValueType will be default, object will be null
        }

        public void Set(string key, object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (value == null) {
                this.CustomDataMap?.Remove(key);
            }
            else {
                if (this.CustomDataMap == null) {
                    this.CustomDataMap = new Dictionary<string, object>();
                }

                this.CustomDataMap[key] = value;
            }
        }

        public void Merge(IDataContext ctx) {
            foreach (object value in ctx.Context) {
                this.ContextList.Add(value);
            }

            if (ctx is DataContext ctxImpl) { // slight optimisation; no need to deconstruct KeyValuePairs into tuples
                if (ctxImpl.CustomDataMap != null && ctxImpl.CustomDataMap.Count > 0) {
                    if (this.CustomDataMap == null) {
                        this.CustomDataMap = new Dictionary<string, object>(ctxImpl.CustomDataMap);
                    }
                    else {
                        foreach (KeyValuePair<string, object> entry in ctxImpl.CustomDataMap) {
                            this.CustomDataMap[entry.Key] = entry.Value;
                        }
                    }
                }
            }
            else {
                List<(string, object)> list = ctx.CustomData.ToList();
                if (list.Count < 1) {
                    return;
                }

                Dictionary<string, object> map = this.CustomDataMap ?? (this.CustomDataMap = new Dictionary<string, object>());
                foreach ((string a, object b) in list) {
                    map[a] = b;
                }
            }
        }
    }
}