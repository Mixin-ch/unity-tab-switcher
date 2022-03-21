using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mixin
{
    [ExecuteInEditMode]
    public class TabSwitcher : MonoBehaviour
    {
        [SerializeField] private bool _autoInit = true;
        [SerializeField] private bool _autoAddButtons = true;
        [SerializeField] private bool _allowClickActivePage = true;
        [SerializeField] private bool _ignorePageObjects = false;
        [SerializeField] private TabColors _tabColors;

        public static event Action<TabSwitchButton> OnAnyTabSwitched;

        [Header("Optional")]
        [SerializeField] private List<TabSwitchButton> _tabSwitchElementList;

        private TabSwitchButton _activePage;

        public TabSwitchButton ActivePage { get => _activePage; }

        /*TEST*/
        private int _activePageInt;
        /*TEST*/


        private void Awake()
        {
            if (_autoInit)
            {
                Setup();
            }
        }

        private void Setup()
        {
            if (_autoAddButtons)
            {
                _tabSwitchElementList.Clear();

                TabSwitchButton[] foundButtons = transform.GetComponentsInChildren<TabSwitchButton>();
                foreach (TabSwitchButton button in foundButtons)
                {
                    _tabSwitchElementList.Add(button);
                }
            }

            // If there are no Buttons, then return
            if (_tabSwitchElementList.Count == 0)
            {
                "Setup failed: No Tab Switch Element added".Log();
                return;
            }

            foreach (TabSwitchButton button in _tabSwitchElementList)
            {
                button.Setup();

                // add button listeners
                button.Button.onClick.AddListener(() =>
                {
                    SwitchToPage(button);
                });

                // Define the Colors
                button.DefineColors(_tabColors);
            }

            // activate first page
            SwitchToPage(_tabSwitchElementList[0]);

        }

        public void SwitchToPage(TabSwitchButton page)
        {
            if (!_allowClickActivePage)
            {
                // If already on this Page, then return
                if (page == _activePage)
                    return;
            }

            $"Switching Page...".Log(Color.yellow);

            // disable all pages
            DeactivateAllPages();

            // activate page
            if (!_ignorePageObjects)
                page.PageObject.SetActive(true);

            // set current active page
            _activePage = page;
            page.IsActive = true;
            page.SetColorAuto();

            // Fire Event
            OnAnyTabSwitched?.Invoke(page);

            // call methods from individual page
            page.OnTabSwitch?.Invoke();
        }

        private void DeactivateAllPages()
        {
            foreach (TabSwitchButton page in _tabSwitchElementList)
                DeactivatePage(page);
        }

        private void DeactivatePage(TabSwitchButton page)
        {
            //deactivate page
            if (!_ignorePageObjects)
                page.PageObject.SetActive(false);

            //set color
            page.IsActive = false;
            page.SetColorAuto();
        }

        private void OnValidate()
        {
            RefreshEditor();
        }

        public void RefreshEditor()
        {
            SwitchToPage(_tabSwitchElementList[_activePageInt]);

            foreach (TabSwitchButton button in _tabSwitchElementList)
            {
                button.Setup();

                // Define the Colors
                button.DefineColors(_tabColors);
            }

            foreach (TabSwitchButton page in _tabSwitchElementList)
                page.SetColorAuto();
        }


        /******TEST*/
        [CustomEditor(typeof(TabSwitcher))]
        [ExecuteInEditMode]
        public class TabSwitcherEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                TabSwitcher tabSwitcher = (TabSwitcher)target;

                List<string> options = new List<string>();
                foreach (TabSwitchButton button in tabSwitcher._tabSwitchElementList)
                {
                    options.Add(button.name);
                }

                EditorGUILayout.BeginHorizontal();
                tabSwitcher._activePageInt = EditorGUILayout.Popup(
                    "Active Page",
                    tabSwitcher._activePageInt,
                    options.ToArray()
                );
                EditorGUILayout.EndHorizontal();


                base.OnInspectorGUI();

                // Handle Realtime Changes
                if (EditorGUI.EndChangeCheck())
                    tabSwitcher.RefreshEditor();
            }
        }
    }



}