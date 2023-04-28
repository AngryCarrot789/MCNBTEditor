using System;
using System.Collections.Generic;
using System.Linq;

namespace MCNBTEditor.Core.Actions.Contexts {
    public class DataContext : IDataContext {
        public Dictionary<string, object> EntryMap { get; set; }
        public List<object> ContextList { get; }

        public IEnumerable<object> Context => this.ContextList;

        public IEnumerable<(string, object)> CustomData => this.EntryMap != null ? this.EntryMap.Select(x => (x.Key, x.Value)) : Enumerable.Empty<(string, object)>();

        public DataContext() {
            this.ContextList = new List<object>();
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

        public bool TryGet<T>(string key, out T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (this.EntryMap != null && this.EntryMap.TryGetValue(key, out object data) && data is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
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
                this.EntryMap?.Remove(key);
            }
            else {
                if (this.EntryMap == null) {
                    this.EntryMap = new Dictionary<string, object>();
                }

                this.EntryMap[key] = value;
            }
        }

        public void Merge(IDataContext ctx) {
            foreach (object o in ctx.Context) {
                this.ContextList.Add(o);
            }

            if (ctx is DataContext ctxImpl) { // slight optimisation; no need to deconstruct KeyValuePairs into tuples
                if (ctxImpl.EntryMap != null && ctxImpl.EntryMap.Count > 0) {
                    Dictionary<string, object> map = this.EntryMap ?? (this.EntryMap = new Dictionary<string, object>());
                    foreach (KeyValuePair<string, object> entry in ctxImpl.EntryMap) {
                        map[entry.Key] = entry.Value;
                    }
                }
            }
            else {
                List<(string, object)> list = ctx.CustomData.ToList();
                if (list.Count <= 0) {
                    return;
                }

                Dictionary<string, object> map = this.EntryMap ?? (this.EntryMap = new Dictionary<string, object>());
                foreach ((string a, object b) in list) {
                    map[a] = b;
                }
            }
        }
    }
}