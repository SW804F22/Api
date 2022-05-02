using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test;

[TraitDiscoverer(GroupDiscoverer.TypeName, TraitDiscovererBase.AssemblyName)]
public class GroupAttribute : Attribute, ITraitAttribute
{
    public string Id { get; set; }
    public GroupAttribute(string id) => Id = id;
    public GroupAttribute() { }
}

public class GroupDiscoverer : TraitDiscovererBase, ITraitDiscoverer
{
    public const string TypeName = TraitDiscovererBase.AssemblyName + ".Helpers.CustomTraits.GroupDiscoverer";

    protected override string CategoryName => "Group";
    public override IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return GetCategory();
        var id = traitAttribute.GetNamedArgument<string>("Id");
        if (!string.IsNullOrEmpty(id))
        {
            yield return new KeyValuePair<string, string>(TypeName, id);
        }
    }
}

public class TraitDiscovererBase : ITraitDiscoverer
{
    public const string AssemblyName = "Nt.Infrastructure.Tests";
    protected const string Category = nameof(Category);
    protected virtual string CategoryName => nameof(CategoryName);

    protected KeyValuePair<string, string> GetCategory()
    {
        return new KeyValuePair<string, string>(Category, CategoryName);
    }
    public virtual IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        return Enumerable.Empty<KeyValuePair<string, string>>();
    }

}
