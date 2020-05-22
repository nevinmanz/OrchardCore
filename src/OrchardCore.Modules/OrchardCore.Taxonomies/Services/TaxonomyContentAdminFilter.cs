using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Services
{
    public class TaxonomyContentAdminFilter : IContentAdminFilter
    {
        private readonly ISiteService _siteService;

        public TaxonomyContentAdminFilter(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task FilterAsync(IQuery<ContentItem> query, ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TaxonomyContentsAdminListSettings>();
            foreach (var contentItemId in settings.TaxonomyContentItemIds)
            {
                var viewModel = new TaxonomyContentAdminFilterViewModel();
                if (await updateModel.TryUpdateModelAsync(viewModel, "Taxonomy" + contentItemId))
                {
                    // Show all items categorized by the taxonomy
                    if (!String.IsNullOrEmpty(viewModel.SelectedContentItemId))
                    {
                        if (viewModel.SelectedContentItemId.StartsWith("Taxonomy:", StringComparison.OrdinalIgnoreCase))
                        {
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(9);
                            query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == viewModel.SelectedContentItemId);
                        }
                        else if (viewModel.SelectedContentItemId.StartsWith("Term:", StringComparison.OrdinalIgnoreCase))
                        {
                            viewModel.SelectedContentItemId = viewModel.SelectedContentItemId.Substring(5);
                            query.With<TaxonomyIndex>(x => x.TermContentItemId == viewModel.SelectedContentItemId);
                        }

                        // We are able to mutate query options when required.
                        // TODO doesn't work
                        model.Options.SelectedContentType = null;
                    }
                }
            }
        }
    }
}
