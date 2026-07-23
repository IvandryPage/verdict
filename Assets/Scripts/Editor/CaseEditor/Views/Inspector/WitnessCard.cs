using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Verdict.Data.Cases;
using Verdict.Editor.CaseEditor.Service;

namespace Verdict.Editor.CaseEditor.Inspector
{
    public sealed class WitnessCard : VisualElement
    {
        private readonly EditorSession session;
        private readonly CaseEditService editService;
        private readonly WitnessContext context;

        public WitnessCard(
            EditorSession session,
            CaseEditService editService,
            WitnessContext context)
        {
            this.session = session;
            this.editService = editService;
            this.context = context;

            style.marginBottom = 12;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            Build();
        }

        private void Build()
        {
            Add(new Label("Witness")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            });

            DrawProperties();

            Add(CreateSeparator());

            DrawTestimonies();
        }

        private void DrawProperties()
        {
            SerializedObject so =
                new SerializedObject(context.Case);

            SerializedProperty witnessProperty =
                so.FindProperty(context.PropertyPath);

            if (witnessProperty == null)
            {
                Add(new Label("Witness property not found."));
                return;
            }

            PropertyField id =
                new(witnessProperty.FindPropertyRelative("id"));

            PropertyField character =
                new(witnessProperty.FindPropertyRelative("character"));

            PropertyField role =
                new(witnessProperty.FindPropertyRelative("role"));

            PropertyField description =
                new(witnessProperty.FindPropertyRelative("description"));

            PropertyField visible =
                new(witnessProperty.FindPropertyRelative("initiallyVisible"));

            id.Bind(so);
            character.Bind(so);
            role.Bind(so);
            description.Bind(so);
            visible.Bind(so);

            Add(id);
            Add(character);
            Add(role);
            Add(description);
            Add(visible);
        }

        private void DrawTestimonies()
        {
            Foldout foldout = new()
            {
                text = $"Testimonies ({context.Witness.Testimonies.Count})",
                value = true
            };

            foreach (TestimonyData testimony in context.Witness.Testimonies)
            {
                TestimonyContext testimonyContext =
                    session.CreateTestimonyContext(
                        context.Witness,
                        testimony);

                foldout.Add(
                    new TestimonyCard(
                        session,
                        editService,
                        testimonyContext));
            }

            Button add =
                new(() =>
                {
                    editService.CreateTestimony(
                        context.Witness);
                })
                {
                    text = "+ Add Testimony"
                };

            foldout.Add(add);

            Add(foldout);
        }

        private static VisualElement CreateSeparator()
        {
            return new VisualElement
            {
                style =
                {
                    height = 1,
                    marginTop = 8,
                    marginBottom = 8,
                    backgroundColor =
                        new Color(.22f,.22f,.22f)
                }
            };
        }
    }
}
