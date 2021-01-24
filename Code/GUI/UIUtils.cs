using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class UIUtils
    {
        // Original utils code by SamsamTS, inherited  here via Ploppable RICO.
        // Altered slightly by algernon for Realistic Population Revisited.
        // SamsamTS's comments:
        // Figuring all this was a pain (no documentation whatsoever)
        // So if your are using it for your mod consider thanking me (SamsamTS)
        // Extended Public Transport UI's code helped me a lot so thanks a lot AcidFire
        //
        // So, thank you, SamsamTS!

        public static UIButton CreateButton(UIComponent parent, float width)
        {
            UIButton button = parent.AddUIComponent<UIButton>();

            button.size = new Vector2(width, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.disabledTextColor = new Color32(128, 128, 128, 255);
            button.canFocus = false;

            return button;
        }


        public static UICheckBox CreateIconToggle(UIComponent parent, string atlas, string checkedSprite, string uncheckedSprite)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            checkBox.width = 35f;
            checkBox.height = 35f;
            checkBox.clipChildren = true;

            UIPanel panel = checkBox.AddUIComponent<UIPanel>();
            panel.backgroundSprite = "IconPolicyBaseRect";
            panel.size = checkBox.size;
            panel.relativePosition = Vector3.zero;

            checkBox.eventCheckChanged += (c, b) =>
            {
                if (checkBox.isChecked)
                    panel.backgroundSprite = "IconPolicyBaseRect";
                else
                    panel.backgroundSprite = "IconPolicyBaseRectDisabled";
                panel.Invalidate();
            };

            checkBox.eventMouseEnter += (c, p) =>
            {
                panel.backgroundSprite = "IconPolicyBaseRectHovered";
            };

            checkBox.eventMouseLeave += (c, p) =>
            {
                if (checkBox.isChecked)
                    panel.backgroundSprite = "IconPolicyBaseRect";
                else
                    panel.backgroundSprite = "IconPolicyBaseRectDisabled";
            };

            UISprite sprite = panel.AddUIComponent<UISprite>();
            sprite.atlas = GetAtlas(atlas);
            sprite.spriteName = uncheckedSprite;
            sprite.size = checkBox.size;
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = sprite.atlas;
            ((UISprite)checkBox.checkedBoxObject).spriteName = checkedSprite;
            checkBox.checkedBoxObject.size = checkBox.size;
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            return checkBox;
        }


        public static void TruncateLabel(UILabel label, float maxWidth)
        {
            label.autoSize = true;
            while (label.width > maxWidth)
            {
                label.text = label.text.Substring(0, label.text.Length - 4) + "...";
                label.autoSize = true;
            }
        }


        public static void ResizeIcon(UISprite icon, Vector2 maxSize)
        {
            icon.width = icon.spriteInfo.width;
            icon.height = icon.spriteInfo.height;

            if (icon.height == 0) return;

            float ratio = icon.width / icon.height;

            if (icon.width > maxSize.x)
            {
                icon.width = maxSize.x;
                icon.height = maxSize.x / ratio;
            }

            if (icon.height > maxSize.y)
            {
                icon.height = maxSize.y;
                icon.width = maxSize.y * ratio;
            }
        }


        public static UITextureAtlas[] s_atlases;


        public static UITextureAtlas GetAtlas(string name)
        {
            if (s_atlases == null)
                s_atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];

            for (int i = 0; i < s_atlases.Length; i++)
            {
                if (s_atlases[i].name == name)
                    return s_atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }
    }
}