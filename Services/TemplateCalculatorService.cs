using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using static TFlex.DOCs.Model.References.Links.RelationTree;

namespace TemplateEngine_v3.Services
{
    public class TemplateCalculatorService
    {
        private readonly TemplateManager _templateManager;
        private readonly TableService _tableService;
        private readonly ExpressionResolver _resolver;

        public TemplateCalculatorService(TemplateManager templateManager)
        {
            _templateManager = templateManager;
            _tableService = templateManager.TableService;
            _resolver = new ExpressionResolver(_tableService);
        }

        public Template Calculate(string orderString, string branchName)
        {
            var template = _templateManager.GetSelectedTemplate().Copy();
            if (!template.Branches.Any(b => b.Name.Equals(branchName)))
                return null;

            var material = ExtractMaterial(orderString);
            var markDictionary = MarkingSpliter.ProcessMarking(orderString, template.ExampleMarkings.ToList());
            _resolver.SetMarkDictionary(markDictionary);

            var relation = SelectValidRelation(template.TemplateRelations);

            if (relation == null)
            {
                template.TemplateRelations = null;
                return template;
            }

            var nodes = FlattenNodes(relation.Nodes);
            _resolver.Prepare(nodes, material, branchName);
            _resolver.ResolveAll();

            ReplaceNodeValues(nodes);
            FilterNodes(nodes);

            relation.Nodes = new (relation.Nodes.Where(node => string.IsNullOrEmpty(node.UsageCondition) || (bool.TryParse(node.UsageCondition, out bool usage) && usage)));
            relation.Designation = _resolver.ReplaceDesignation(relation.Designation);
            relation.IsLoggingEnabled = true;
            template.TemplateRelations.Clear();
            template.TemplateRelations.Add(relation);
            return template;
        }

        private string ExtractMaterial(string order)
        {
            var match = Regex.Match(order, @"RAL\d+", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var formatted = Regex.Replace(match.Value, @"RAL\s*(\d+)", "RAL $1");
                return _templateManager.MaterialManager.GetMaterials()
                       .FirstOrDefault(m => m.Contains(formatted));
            }
            return string.Empty;
        }

        private TemplateRelations SelectValidRelation(IEnumerable<TemplateRelations> relations)
        {
            foreach (var rel in relations)
            {
                rel.IsLoggingEnabled = false;
                rel.UsageCondition = _resolver.Calculate(rel.UsageCondition)?.ToString();
                if (string.IsNullOrEmpty(rel.UsageCondition) || bool.TryParse(rel.UsageCondition, out var used) && used)
                    return rel;
            }
            return null;
        }

        private List<Node> FlattenNodes(IEnumerable<Node> rootNodes)
        {
            var allNodes = new List<Node>();
            foreach (var node in rootNodes)
            {
                node.IsLoggingEnabled = false;
                allNodes.Add(node);
                allNodes.AddRange(FlattenNodes(node.Nodes));
            }
            return allNodes;
        }

        private void ReplaceNodeValues(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                node.Designation = _resolver.ReplaceDesignation(node.Designation);
                node.Name = _resolver.ReplaceDesignation(node.Name);
                node.UsageCondition = _resolver.Calculate(node.UsageCondition)?.ToString();
                node.IsLoggingEnabled = true;
            }
        }

        private void FilterNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes.ToList())
            {
                if (!string.IsNullOrEmpty(node.UsageCondition) && bool.TryParse(node.UsageCondition, out var use) && !use)
                    nodes.ToList().Remove(node);
            }
        }
    }

}
