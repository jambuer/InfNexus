using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using GDS.Demos.Basic.Views;
using GDS.Core;
using UnityEngine;

namespace GDS.Demos.Basic
{

    [CustomPropertyDrawer(typeof(ListItemDto))]
    public class ItemDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            if (property.managedReferenceValue == null)
            {
                property.managedReferenceValue = new ListItemDto() { baseId = Bases.Mushroom.Id };
                property.serializedObject.ApplyModifiedProperties();
            }

            var baseIdProp = property.FindPropertyRelative(nameof(ListItemDto.baseId));
            var rarityProp = property.FindPropertyRelative(nameof(ListItemDto.rarity));
            var quantProp = property.FindPropertyRelative(nameof(ListItemDto.quant));




            var itemView = new BasicItemView() { Data = ItemFactory.Create(BasesExt.Get(baseIdProp.stringValue), (Rarity)rarityProp.enumValueFlag) };
            var baseField = new PopupField<string>("Base", BasesExt.AllIds.ToList(), 0) { bindingPath = baseIdProp.propertyPath };
            var rarityField = new EnumField("Rarity") { bindingPath = rarityProp.propertyPath };
            var quantField = new IntegerField("Quantity", 3) { bindingPath = quantProp.propertyPath };

            quantField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 1) { quantField.value = 1; }
                itemView.Data = itemView.Data as BasicItem with { Quant = evt.newValue };
            });

            rarityField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null) return;
                itemView.Data = itemView.Data as BasicItem with { Rarity = (Rarity)evt.newValue };
            });

            baseField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null) return;
                var itemBase = BasesExt.Get(evt.newValue);
                if (itemBase.Stack is not NoStack)
                {
                    quantField.SetEnabled(true);
                    rarityField.SetEnabled(false);
                    rarityField.value = Rarity.NoRarity;
                }
                else
                {
                    quantField.SetEnabled(false);
                    quantField.value = 1;
                    rarityField.SetEnabled(true);
                }

                itemView.Data = itemView.Data with { Base = itemBase };

            });


            var container = Dom.Div("row",
                itemView,
                Dom.Div("flex-grow-1",
                    baseField,
                    rarityField,
                    quantField
                )
            );

            container.styleSheets.Add(Resources.Load<StyleSheet>("Basic/BasicStyles"));
            container.styleSheets.Add(Resources.Load<StyleSheet>("Shared/Styles/Utility"));

            return container;
        }
    }
}