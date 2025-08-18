using Bible.Data;
using Bible.Usx.Models;
using Bible.Usx.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Wasm.Services
{
    /// <summary>
    /// Visitor that builds RenderFragment from UsjBook nodes.
    /// </summary>
    [Obsolete("Use UsjRenderer.razor instead.")]
    internal class UsjRenderVisitor : IUsjVisitor
    {
        private readonly RenderFragmentBuilder _builder = new();
        private readonly UsxToUsjConverter _usxToUsjConverter;
        private readonly ILogger<UsjRenderVisitor> _logger;

        public UsjRenderVisitor(ILogger<UsjRenderVisitor>? logger = null, UsxToUsjConverter? usxToUsjConverter = null)
        {
            _logger = logger ?? NullLogger<UsjRenderVisitor>.Instance;
            _usxToUsjConverter = usxToUsjConverter ?? new UsxToUsjConverter();
        }

        public void Visit(UsjChapterMarker marker)
        {
            if (string.IsNullOrEmpty(marker.Number)) return;
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "h3");
                    builder.AddAttribute(1, "class", $"chapter-marker {marker.Style}");
                    builder.AddContent(2, $"Chapter {marker.Number}");
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjChapterMarker node: {@Node}", marker);
                throw;
            }
        }

        public void Visit(UsjVerseMarker marker)
        {
            if (string.IsNullOrEmpty(marker.Number)) return;
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "sup");
                    builder.AddAttribute(1, "class", $"verse-marker {marker.Style}");
                    builder.AddContent(2, marker.Number);
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjVerseMarker node: {@Node}", marker);
                throw;
            }
        }

        public void Visit(UsjIdentification identification)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "section");
                    builder.AddAttribute(1, "class", "usj-identification");
                    builder.OpenElement(2, "p");
                    builder.AddContent(3, $"{identification.VersionDescription} - {identification.BookCode}");
                    builder.CloseElement();
                    builder.OpenElement(4, "p");
                    builder.CloseElement();
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjIdentification node: {@Node}", identification);
                throw;
            }
        }

        private static readonly IReadOnlyList<string> ParaStylesToHide =
            UsjToMarkdownVisitor.ParaStylesToHide;

        public void Visit(UsjPara para)
        {
            if (!string.IsNullOrEmpty(para.Style) &&
                para.Style.StartsWith("h", StringComparison.OrdinalIgnoreCase) &&
                para.Text is string heading)
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "h2");
                    builder.AddContent(1, $"{heading}");
                    builder.CloseElement();
                });
            }
            else if (string.IsNullOrEmpty(para.Style) ||
                !ParaStylesToHide.Any(p => para.Style.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                // Build child fragments first
                var childFragments = new List<RenderFragment>();
                if (para.Content != null)
                {
                    foreach (var node in para.Content)
                    {
                        var visitor = new UsjRenderVisitor(_logger, _usxToUsjConverter); // or reuse this if possible
                        childFragments.Add(visitor.Build(node));
                    }
                }
                //Build(para);

                // Now add the para element with all children fragments inside
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "p");
                    builder.AddAttribute(1, "class", $"usj-para {para.Style}");
                    //this.Accept(para.Content);
                    foreach (var childFragment in childFragments)
                    {
                        builder.AddContent(2, childFragment);
                    }

                    builder.CloseElement();
                });
            }
        }

        public void Visit(UsjChar metatext)
        {
            if (metatext == null) return;
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", $"usj-char {metatext.Style}");
                    if (!string.IsNullOrEmpty(metatext.Metadata))
                    {
                        builder.AddAttribute(2, "data-strong", metatext.Metadata);
                    }
                    builder.AddContent(3, metatext.Text);
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjChar node: {@Node}", metatext);
                throw;
            }
        }

        public void Visit(UsjText text)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.AddContent(1, text.Text);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjText node: {@Node}", text);
                throw;
            }
        }

        public void Visit(UsjMilestone milestone)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", $"usj-milestone {milestone.Style}");
                    builder.AddContent(2, "[Milestone]");
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjMilestone node: {@Node}", milestone);
                throw;
            }
        }

        public void Visit(UsjLineBreak lineBreak)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "br");
                    builder.AddAttribute(1, "class", $"usj-linebreak {lineBreak.Style}");
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjLineBreak node: {@Node}", lineBreak);
                throw;
            }
        }

        public void Visit(UsjCrossReference reference)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "a");
                    builder.AddAttribute(1, "href", $"#{reference.Location}");
                    builder.AddAttribute(2, "class", $"usj-crossref {reference.Style}");

                    if (reference.Content != null)
                    {
                        foreach (var node in reference.Content)
                        {
                            try
                            {
                                this.Accept(node);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error rendering child node in UsjCrossReference: {@ChildNode}", node);
                                throw;
                            }
                        }
                    }

                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjCrossReference node: {@Node}", reference);
                throw;
            }
        }

        public void Visit(UsjFootnote footnote)
        {
            try
            {
                _builder.AddContent(0, builder =>
                {
                    builder.OpenElement(0, "sup");
                    builder.AddAttribute(1, "class", $"usj-footnote-caller {footnote.Style}");
                    builder.AddContent(2, footnote.Caller);
                    builder.CloseElement();

                    builder.OpenElement(3, "aside");
                    builder.AddAttribute(4, "class", $"usj-footnote-content {footnote.Style}");
                    if (footnote.Content != null)
                    {
                        foreach (var node in footnote.Content)
                        {
                            try
                            {
                                this.Accept(node);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error rendering child node in UsjFootnote: {@ChildNode}", node);
                                throw;
                            }
                        }
                    }
                    builder.CloseElement();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering UsjFootnote node: {@Node}", footnote);
                throw;
            }
        }

        public RenderFragment Build(IUsjNode? usjNode)
        {
            try
            {
                this.Accept(usjNode);
                return _builder.Build();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building RenderFragment for UsjBook");
                throw;
            }
        }

        public async Task<UsjBook> DeserializeAsync(string? isoLanguage, string? bibleVersion, string? bookCode, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var usxStream = ResourceHelper.GetUsxBookStream(isoLanguage, bibleVersion, bookCode);
                return await _usxToUsjConverter.ConvertUsxStreamToUsjBookAsync(usxStream, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing USX to USJ book");
                throw;
            }
        }

        public async Task<RenderFragment> DeserializeRenderFragmentAsync(string? isoLanguage, string? bibleVersion, string? bookCode, CancellationToken cancellationToken = default)
        {
            var usjBook = await DeserializeAsync(isoLanguage, bibleVersion, bookCode, cancellationToken);
            return Build(usjBook);
        }

        /// <summary>
        /// Helper class to accumulate RenderFragments.
        /// </summary>
        class RenderFragmentBuilder
        {
            private readonly List<RenderFragment> _fragments = new();

            public void AddContent(int seq, RenderFragment fragment)
            {
                _fragments.Add(fragment);
            }

            public RenderFragment Build()
            {
                var fragmentsSnapshot = _fragments.ToArray();
                return builder =>
                {
                    foreach (var fragment in fragmentsSnapshot)
                    {
                        fragment(builder);
                    }
                };
            }
        }
    }
}