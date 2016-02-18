using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetRenderingDatasource;
using Sitecore.SecurityModel;
using Sitecore.Text;

namespace Sc.Commons.EnsureDatasourceLocations
{
    public class EnsureDatasourceLocationsProcessor
    {
        private readonly int _sortOrder;
        private readonly ID _templateId;

        public EnsureDatasourceLocationsProcessor(string sortOrder, string templateId)
        {
            _templateId = ID.Parse(templateId);
            _sortOrder = int.Parse(sortOrder);
        }

        public void Process(GetRenderingDatasourceArgs args)
        {
            Assert.IsNotNull(args, "args");

            var database = args.ContentDatabase;

            using (new SecurityDisabler())
            {
                foreach (var dataSourceLocation in new ListString(args.RenderingItem["Datasource Location"], '|'))
                {
                    //skip queries as we cant determine the path
                    if (!dataSourceLocation.StartsWith("query:"))
                    {
                        var privatePath = dataSourceLocation;

                        if (dataSourceLocation.StartsWith("./", StringComparison.InvariantCulture) && !string.IsNullOrEmpty(args.ContextItemPath))
                            privatePath = args.ContextItemPath + dataSourceLocation.Remove(0, 1);

                        var privateItem = database.SelectSingleItem(privatePath);

                        if (privateItem == null)
                        {
                            var template = database.GetTemplate(_templateId);
                            privateItem = database.CreateItemPath(privatePath, template);

                            using (new EditContext(privateItem, false, true))
                            {
                                privateItem.Appearance.Sortorder = _sortOrder;
                            }
                        }
                    }
                }
            }

        }
    }
}
