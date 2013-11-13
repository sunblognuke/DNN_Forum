using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Modules.Forum;
using Satrabel.HttpModules.Provider;

namespace SunBlogNuke.Providers.OpenUrlRewriter
{
    public class ForumUrlRuleProvider : UrlRuleProvider
    {
        private const string ProviderType = "urlRule";
        private const string ProviderName = "ForumUrlRuleProvider";
        private const string ForumModule_FriendlyName = "Forum";

        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly bool includePageName = false;

        public ForumUrlRuleProvider()
        {
            var objProvider = (DotNetNuke.Framework.Providers.Provider)_providerConfiguration.Providers[ProviderName];
            if (!String.IsNullOrEmpty(objProvider.Attributes["includePageName"]))
            {
                includePageName = bool.Parse(objProvider.Attributes["includePageName"]);
            }
        }

        public override List<UrlRule> GetRules(int PortalId)
        {
            List<UrlRule> Rules = new List<UrlRule>();

            ThreadController cntThread = new ThreadController();
            List<ThreadInfo> allPosts = cntThread.GetSitemapThreads(PortalId);
            ModuleController mc = new ModuleController();
            ArrayList modules = mc.GetModulesByDefinition(PortalId, ForumModule_FriendlyName);
            foreach (ModuleInfo module in modules.OfType<ModuleInfo>())
            {
                foreach (var article in allPosts)
                {
                    var rule = new UrlRule
                    {
                        CultureCode = module.CultureCode,
                        TabId = module.TabID,
                        RuleType = UrlRuleType.Module,
                        Parameters = "threadid=" + article.ThreadID,
                        Action = UrlRuleAction.Rewrite,
                        Url = CleanupUrl(article.Subject),
                        RemoveTab = !includePageName
                    };
                    Rules.Add(rule);
                }
            }

            return Rules;
        }
    }
}