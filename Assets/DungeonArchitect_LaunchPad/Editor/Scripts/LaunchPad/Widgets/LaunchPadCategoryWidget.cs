using DungeonArchitect.UI;
using DungeonArchitect.UI.Widgets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Editors.LaunchPad
{
    public struct LaunchPadCategoryData
    {
        public LaunchPadCategoryData(string path, string displayText)
        {
            this.path = path;
            this.displayText = displayText;
        }

        public string path;
        public string displayText;
    }

    public class LaunchPadCategoryDataSource : ListViewSource<LaunchPadCategoryData>
    {
        LaunchPadCategoryData[] items;
        public void SetItems(LaunchPadCategoryData[] items)
        {
            this.items = items;
        }

        public override LaunchPadCategoryData[] GetItems()
        {
            return items;
        }

        public override IWidget CreateWidget(LaunchPadCategoryData item)
        {
            var itemWidget = new LaunchPadCategoryItem(item);
            itemWidget.TextStyle.fontSize = 16;

            itemWidget.SelectedTextStyle = new GUIStyle(itemWidget.TextStyle);
            itemWidget.SelectedTextStyle.normal.textColor = Color.white;
            itemWidget.SelectedColor = new Color(0.2f, 0.2f, 0.2f);

            return itemWidget;
        }
    }

    public class LaunchPadCategoryItem : ListViewTextItemWidget
    {
        public LaunchPadCategoryItem(LaunchPadCategoryData category)
            : base(category, () => category.displayText)
        {
        }
    }
}
