using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    internal sealed class GridColumnEditorDefinition
    {
        public Control Editor { get; set; }
        public GridColumnEditorMetadata Metadata { get; set; }
        public IGridEditorValueValidator Validator { get; set; }
        public IGridEditorValueNormalizer Normalizer { get; set; }
        public DataRow EditingRow { get; set; }
        public object OriginalDataValue { get; set; }
        public string OriginalDisplayText { get; set; }
    }

    internal sealed class GridColumnEditorRegistry
    {
        private readonly Dictionary<Control, GridColumnEditorDefinition> _definitionsByEditor =
                         new Dictionary<Control, GridColumnEditorDefinition>();

        private readonly Dictionary<string, GridColumnEditorDefinition> _definitionsByColumnName =
                         new Dictionary<string, GridColumnEditorDefinition>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<GridColumnEditorDefinition> Definitions => _definitionsByColumnName.Values;

        public void Register(GridColumnEditorDefinition definition)
        {
            if (definition == null || definition.Metadata == null || string.IsNullOrWhiteSpace(definition.Metadata.ColumnName))
            {
                return;
            }

            _definitionsByColumnName[definition.Metadata.ColumnName] = definition;

            if (definition.Editor != null)
            {
                _definitionsByEditor[definition.Editor] = definition;
            }
        }

        public bool TryGet(Control editor, out GridColumnEditorDefinition definition)
        {
            definition = null;
            return editor != null && _definitionsByEditor.TryGetValue(editor, out definition);
        }

        public bool TryGet(string columnName, out GridColumnEditorDefinition definition)
        {
            definition = null;
            return !string.IsNullOrWhiteSpace(columnName) && _definitionsByColumnName.TryGetValue(columnName, out definition);
        }

        public void Clear()
        {
            _definitionsByEditor.Clear();
            _definitionsByColumnName.Clear();
        }
    }
}