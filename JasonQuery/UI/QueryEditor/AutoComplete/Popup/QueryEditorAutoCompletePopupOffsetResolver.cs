namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal static class QueryEditorAutoCompletePopupOffsetResolver
    {
        public static int Resolve(string queryEditorFontSize, int editorZoom)
        {
            var yShift = 0;

            switch (queryEditorFontSize)
            {
                case "10":
                    {
                        switch (editorZoom)
                        {
                            case 20: yShift = 2; break;
                            case 19: yShift = 3; break;
                            case 18: yShift = 6; break;
                            case 17: yShift = 7; break;
                            case 16: yShift = 8; break;
                            case 15: yShift = 11; break;
                            case 14: yShift = 12; break;
                            case 13: yShift = 12; break;
                            case 12: yShift = 15; break;
                            case 11: yShift = 16; break;
                            case 10: yShift = 17; break;
                            case 9: yShift = 21; break;
                            case 8: yShift = 21; break;
                            case 7: yShift = 22; break;
                            case 6: yShift = 25; break;
                            case 5: yShift = 26; break;
                            case 4: yShift = 27; break;
                            case 3: yShift = 29; break;
                            case 2: yShift = 30; break;
                            case 1: yShift = 31; break;
                            case 0: yShift = 35; break;
                            case -1: yShift = 36; break;
                            case -2: yShift = 37; break;
                            case -3: yShift = 40; break;
                            case -4: yShift = 41; break;
                            case -5: yShift = 42; break;
                            case -6: yShift = 44; break;
                            case -7: yShift = 45; break;
                            case -8: yShift = 46; break;
                            case -9: yShift = 46; break;
                            case -10: yShift = 46; break;
                        }

                        break;
                    }
                case "12":
                    {
                        switch (editorZoom)
                        {
                            case 20: yShift = -2; break;
                            case 19: yShift = 1; break;
                            case 18: yShift = 2; break;
                            case 17: yShift = 3; break;
                            case 16: yShift = 6; break;
                            case 15: yShift = 7; break;
                            case 14: yShift = 8; break;
                            case 13: yShift = 11; break;
                            case 12: yShift = 12; break;
                            case 11: yShift = 12; break;
                            case 10: yShift = 15; break;
                            case 9: yShift = 16; break;
                            case 8: yShift = 17; break;
                            case 7: yShift = 20; break;
                            case 6: yShift = 21; break;
                            case 5: yShift = 22; break;
                            case 4: yShift = 25; break;
                            case 3: yShift = 26; break;
                            case 2: yShift = 28; break;
                            case 1: yShift = 30; break;
                            case 0: yShift = 31; break;
                            case -1: yShift = 32; break;
                            case -2: yShift = 35; break;
                            case -3: yShift = 36; break;
                            case -4: yShift = 37; break;
                            case -5: yShift = 40; break;
                            case -6: yShift = 41; break;
                            case -7: yShift = 42; break;
                            case -8: yShift = 44; break;
                            case -9: yShift = 45; break;
                            case -10: yShift = 47; break;
                        }

                        break;
                    }
                case "14":
                    {
                        switch (editorZoom)
                        {
                            case 20: yShift = -3; break;
                            case 19: yShift = -2; break;
                            case 18: yShift = -2; break;
                            case 17: yShift = 1; break;
                            case 16: yShift = 2; break;
                            case 15: yShift = 3; break;
                            case 14: yShift = 6; break;
                            case 13: yShift = 7; break;
                            case 12: yShift = 8; break;
                            case 11: yShift = 11; break;
                            case 10: yShift = 12; break;
                            case 9: yShift = 12; break;
                            case 8: yShift = 15; break;
                            case 7: yShift = 16; break;
                            case 6: yShift = 17; break;
                            case 5: yShift = 20; break;
                            case 4: yShift = 21; break;
                            case 3: yShift = 22; break;
                            case 2: yShift = 25; break;
                            case 1: yShift = 26; break;
                            case 0: yShift = 28; break;
                            case -1: yShift = 30; break;
                            case -2: yShift = 30; break;
                            case -3: yShift = 32; break;
                            case -4: yShift = 34; break;
                            case -5: yShift = 36; break;
                            case -6: yShift = 37; break;
                            case -7: yShift = 40; break;
                            case -8: yShift = 41; break;
                            case -9: yShift = 42; break;
                            case -10: yShift = 44; break;
                        }

                        break;
                    }
                case "16":
                    {
                        switch (editorZoom)
                        {
                            case 20: yShift = -7; break;
                            case 19: yShift = -6; break;
                            case 18: yShift = -3; break;
                            case 17: yShift = -2; break;
                            case 16: yShift = -2; break;
                            case 15: yShift = 0; break;
                            case 14: yShift = 1; break;
                            case 13: yShift = 3; break;
                            case 12: yShift = 6; break;
                            case 11: yShift = 7; break;
                            case 10: yShift = 8; break;
                            case 9: yShift = 11; break;
                            case 8: yShift = 12; break;
                            case 7: yShift = 12; break;
                            case 6: yShift = 15; break;
                            case 5: yShift = 16; break;
                            case 4: yShift = 17; break;
                            case 3: yShift = 20; break;
                            case 2: yShift = 21; break;
                            case 1: yShift = 22; break;
                            case 0: yShift = 25; break;
                            case -1: yShift = 26; break;
                            case -2: yShift = 27; break;
                            case -3: yShift = 29; break;
                            case -4: yShift = 30; break;
                            case -5: yShift = 31; break;
                            case -6: yShift = 34; break;
                            case -7: yShift = 35; break;
                            case -8: yShift = 36; break;
                            case -9: yShift = 40; break;
                            case -10: yShift = 41; break;
                        }

                        break;
                    }
                case "18":
                    {
                        switch (editorZoom)
                        {
                            case 20: yShift = -12; break;
                            case 19: yShift = -8; break;
                            case 18: yShift = -7; break;
                            case 17: yShift = -6; break;
                            case 16: yShift = -4; break;
                            case 15: yShift = -3; break;
                            case 14: yShift = -2; break;
                            case 13: yShift = 1; break;
                            case 12: yShift = 2; break;
                            case 11: yShift = 3; break;
                            case 10: yShift = 6; break;
                            case 9: yShift = 7; break;
                            case 8: yShift = 8; break;
                            case 7: yShift = 11; break;
                            case 6: yShift = 12; break;
                            case 5: yShift = 12; break;
                            case 4: yShift = 15; break;
                            case 3: yShift = 16; break;
                            case 2: yShift = 17; break;
                            case 1: yShift = 20; break;
                            case 0: yShift = 21; break;
                            case -1: yShift = 22; break;
                            case -2: yShift = 25; break;
                            case -3: yShift = 26; break;
                            case -4: yShift = 27; break;
                            case -5: yShift = 30; break;
                            case -6: yShift = 31; break;
                            case -7: yShift = 32; break;
                            case -8: yShift = 35; break;
                            case -9: yShift = 36; break;
                            case -10: yShift = 37; break;
                        }

                        break;
                    }
            }

            return yShift;
        }
    }
}