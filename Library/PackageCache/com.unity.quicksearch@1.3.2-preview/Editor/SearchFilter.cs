using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Unity.QuickSearch
{
    public class SearchFilter
    {
        [DebuggerDisplay("{name.displayName}")]
        public class Entry
        {
            public Entry(NameId name)
            {
                this.name = name;
                isEnabled = true;
            }

            public NameId name;
            public bool isEnabled;
        }

        [DebuggerDisplay("{entry.name.displayName} expanded:{isExpanded}")]
        public class ProviderDesc
        {
            public ProviderDesc(NameId name, SearchProvider provider)
            {
                entry = new Entry(name);
                categories = new List<Entry>();
                isExpanded = false;
                this.provider = provider;
            }

            public int priority => provider.priority;

            public Entry entry;
            public bool isExpanded;
            public List<Entry> categories;
            public SearchProvider provider;
        }

        public bool allActive { get; internal set; }

        public List<SearchProvider> filteredProviders;
        public List<ProviderDesc> providerFilters;

        private List<SearchProvider> m_Providers;
        public List<SearchProvider> Providers
        {
            get => m_Providers;

            set
            {
                m_Providers = value;
                providerFilters.Clear();
                filteredProviders.Clear();
                foreach (var provider in m_Providers.Where(p => p.active))
                {
                    var providerFilter = new ProviderDesc(new NameId(provider.name.id, GetProviderNameWithFilter(provider)), provider);
                    providerFilters.Add(providerFilter);
                    foreach (var subCategory in provider.subCategories)
                    {
                        providerFilter.categories.Add(new Entry(subCategory));
                    }
                }
                UpdateFilteredProviders();
            }
        }

        public SearchFilter()
        {
            filteredProviders = new List<SearchProvider>();
            providerFilters = new List<ProviderDesc>();
        }

        public void ResetFilter(bool enableAll, bool preserveSubFilters = false)
        {
            allActive = enableAll;
            foreach (var providerDesc in providerFilters)
                SetFilterInternal(enableAll, providerDesc.entry.name.id, null, preserveSubFilters);
            UpdateFilteredProviders();
        }

        public void SetFilter(bool isEnabled, string providerId, string subCategory = null, bool preserveSubFilters = false)
        {
            if (SetFilterInternal(isEnabled, providerId, subCategory, preserveSubFilters))
                UpdateFilteredProviders();
        }

        public void SetExpanded(bool isExpanded, string providerId)
        {
            var providerDesc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (providerDesc != null)
            {
                providerDesc.isExpanded = isExpanded;
            }
        }

        public bool IsEnabled(string providerId, string subCategory = null)
        {
            var desc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (desc != null)
            {
                if (subCategory == null)
                {
                    return desc.entry.isEnabled;
                }

                foreach (var cat in desc.categories)
                {
                    if (cat.name.id == subCategory)
                        return cat.isEnabled;
                }
            }

            return false;
        }

        public static string GetProviderNameWithFilter(SearchProvider provider)
        {
            return string.IsNullOrEmpty(provider.filterId) ? provider.name.displayName : provider.name.displayName + " (" + provider.filterId + ")";
        }

        public List<Entry> GetSubCategories(SearchProvider provider)
        {
            var desc = providerFilters.Find(pd => pd.entry.name.id == provider.name.id);
            return desc?.categories;
        }

        internal void UpdateFilteredProviders()
        {
            filteredProviders = Providers.Where(p => IsEnabled(p.name.id)).ToList();
        }

        internal bool SetFilterInternal(bool isEnabled, string providerId, string subCategory = null, bool preserveSubFilters = false)
        {
            var providerDesc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (providerDesc == null) 
                return false;

            if (subCategory == null)
            {
                providerDesc.entry.isEnabled = isEnabled;
                if (preserveSubFilters) 
                    return true;

                foreach (var cat in providerDesc.categories)
                    cat.isEnabled = isEnabled;
            }
            else
            {
                foreach (var cat in providerDesc.categories)
                {
                    if (cat.name.id == subCategory)
                    {
                        cat.isEnabled = isEnabled;
                        if (isEnabled)
                            providerDesc.entry.isEnabled = true;
                    }
                }
            }

            return true;

        }
    }
}