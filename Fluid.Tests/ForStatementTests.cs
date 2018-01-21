using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Ast;
using Fluid.Values;
using Xunit;

namespace Fluid.Tests
{
    public class ForStatementTests
    {
        [Fact]
        public async Task ShouldMaintainVariableValues()
        {
            const string expectedValue = "New Value";

            var context = new TemplateContext();
            var a = new AssignStatement("a", new LiteralExpression(new StringValue("Original Value")));

            var loop = new ForStatement(
                new[] { new AssignStatement("a", new LiteralExpression(new StringValue(expectedValue))) },
                "i",
                new RangeExpression(
                    new LiteralExpression(new NumberValue(1)),
                    new LiteralExpression(new NumberValue(2))
                ),
                null, null, false
            );
            var sw = new StringWriter();
            await a.WriteToAsync(sw, HtmlEncoder.Default, context);
            await loop.WriteToAsync(sw, HtmlEncoder.Default, context);

            FluidTemplate.TryParse("{{a}}", out var template, out var messages);
            
            var result = await template.RenderAsync(context);

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task ShouldLoopRange()
        {
            var e = new ForStatement(
                new[] { new TextStatement("x") },
                "i",
                new RangeExpression(
                    new LiteralExpression(new NumberValue(1)),
                    new LiteralExpression(new NumberValue(3))
                ),
                null, null, false
            );

            var sw = new StringWriter();
            await e.WriteToAsync(sw, HtmlEncoder.Default, new TemplateContext());

            Assert.Equal("xxx", sw.ToString());
        }

        [Fact]
        public async Task ShouldLoopArrays()
        {
            var e = new ForStatement(
                new[] { new TextStatement("x") },
                "i",
                new MemberExpression(
                    new IdentifierSegment("items")
                ),
                null, null, false
            );

            var sw = new StringWriter();
            var context = new TemplateContext();
            context.SetValue("items", new[] { 1, 2, 3 });
            await e.WriteToAsync(sw, HtmlEncoder.Default, context);

            Assert.Equal("xxx", sw.ToString());
        }

        [Fact]
        public async Task ShouldHandleBreak()
        {
            var e = new ForStatement(
                new Statement[] {
                    new TextStatement("x"),
                    new BreakStatement(),
                    new TextStatement("y")
                },
                "i",
                new MemberExpression(
                    new IdentifierSegment("items")
                ),
                null, null, false
            );

            var sw = new StringWriter();
            var context = new TemplateContext();
            context.SetValue("items", new[] { 1, 2, 3 });
            await e.WriteToAsync(sw, HtmlEncoder.Default, context);

            Assert.Equal("x", sw.ToString());
        }

        [Fact]
        public async Task ShouldHandleContinue()
        {
            var e = new ForStatement(
                new Statement[] {
                    new TextStatement("x"),
                    new ContinueStatement(),
                    new TextStatement("y")
                },
                "i",
                new MemberExpression(
                    new IdentifierSegment("items")
                ),
                null, null, false
            );

            var sw = new StringWriter();
            var context = new TemplateContext();
            context.SetValue("items", new[] { 1, 2, 3 });
            await e.WriteToAsync(sw, HtmlEncoder.Default, context);

            Assert.Equal("xxx", sw.ToString());
        }

        [Fact]
        public async Task ForShouldProvideHelperVariables()
        {

            var e = new ForStatement(
                new Statement[] {
                    CreateMemberStatement("forloop.length"),
                    CreateMemberStatement("forloop.index"),
                    CreateMemberStatement("forloop.index0"),
                    CreateMemberStatement("forloop.rindex"),
                    CreateMemberStatement("forloop.rindex0"),
                    CreateMemberStatement("forloop.first"),
                    CreateMemberStatement("forloop.last")
                },
                "i",
                new MemberExpression(
                    new IdentifierSegment("items")
                ),
                null, null, false
            );

            var sw = new StringWriter();
            var context = new TemplateContext();
            context.SetValue("items", new[] { 1, 2, 3 });
            await e.WriteToAsync(sw, HtmlEncoder.Default, context);

            Assert.Equal("31023truefalse32112falsefalse33201falsetrue", sw.ToString());
        }

        [Fact]
        public async Task ForEvaluatesOptions()
        {
            var e = new ForStatement(
                new[] { CreateMemberStatement("i") },
                "i",
                new RangeExpression(
                    new LiteralExpression(new NumberValue(1)),
                    new LiteralExpression(new NumberValue(5))
                ),
                new LiteralExpression(new NumberValue(3)),
                new LiteralExpression(new NumberValue(2)),
                true
            );

            var sw = new StringWriter();
            await e.WriteToAsync(sw, HtmlEncoder.Default, new TemplateContext());

            Assert.Equal("543", sw.ToString());
        }


        Statement CreateMemberStatement(string p)
        {
            return new OutputStatement(new MemberExpression(p.Split('.').Select(x => new IdentifierSegment(x)).ToArray()));
        }
    }
}
