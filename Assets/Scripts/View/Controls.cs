// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Control/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Board"",
            ""id"": ""9cb5934b-4d99-4e27-8cfa-284b3e7efc62"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""fa53dd31-deb0-4309-8cfa-9f2fb7a23387"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""PassThrough"",
                    ""id"": ""413ded6f-d8cf-4dc8-8f0e-5b388efe207a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""Button"",
                    ""id"": ""a997b3a8-dbfb-46f1-8f0f-a8c2203ef948"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Escape Menu"",
                    ""type"": ""Button"",
                    ""id"": ""7d0f2c6a-23c5-462a-9549-23f2333645b2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a6db2303-7b53-4fd7-8e80-78371c1f7289"",
                    ""path"": ""<Mouse>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d1710ced-fa06-43be-9b51-e9edcd652780"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3980081e-e6bd-4efc-8abc-4bc7b0cb5fbe"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3677fee5-6ca0-46ad-974b-0c1596d49fa2"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Escape Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu"",
            ""id"": ""78d797a2-dd7b-4461-b05f-de57f8d79127"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""c3985450-017b-4882-b285-4c0fbe650405"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Escape Menu"",
                    ""type"": ""Button"",
                    ""id"": ""fe787e5c-376d-4cfe-8c85-d322275b8eb2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0d9d020a-1cdf-442e-b676-fab1d293843f"",
                    ""path"": ""<Mouse>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""234e48fb-53bf-42e1-859c-079bbe05d3ff"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Escape Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Board
        m_Board = asset.FindActionMap("Board", throwIfNotFound: true);
        m_Board_Click = m_Board.FindAction("Click", throwIfNotFound: true);
        m_Board_Point = m_Board.FindAction("Point", throwIfNotFound: true);
        m_Board_RightClick = m_Board.FindAction("RightClick", throwIfNotFound: true);
        m_Board_EscapeMenu = m_Board.FindAction("Escape Menu", throwIfNotFound: true);
        // Menu
        m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
        m_Menu_Click = m_Menu.FindAction("Click", throwIfNotFound: true);
        m_Menu_EscapeMenu = m_Menu.FindAction("Escape Menu", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Board
    private readonly InputActionMap m_Board;
    private IBoardActions m_BoardActionsCallbackInterface;
    private readonly InputAction m_Board_Click;
    private readonly InputAction m_Board_Point;
    private readonly InputAction m_Board_RightClick;
    private readonly InputAction m_Board_EscapeMenu;
    public struct BoardActions
    {
        private @Controls m_Wrapper;
        public BoardActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_Board_Click;
        public InputAction @Point => m_Wrapper.m_Board_Point;
        public InputAction @RightClick => m_Wrapper.m_Board_RightClick;
        public InputAction @EscapeMenu => m_Wrapper.m_Board_EscapeMenu;
        public InputActionMap Get() { return m_Wrapper.m_Board; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BoardActions set) { return set.Get(); }
        public void SetCallbacks(IBoardActions instance)
        {
            if (m_Wrapper.m_BoardActionsCallbackInterface != null)
            {
                @Click.started -= m_Wrapper.m_BoardActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_BoardActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_BoardActionsCallbackInterface.OnClick;
                @Point.started -= m_Wrapper.m_BoardActionsCallbackInterface.OnPoint;
                @Point.performed -= m_Wrapper.m_BoardActionsCallbackInterface.OnPoint;
                @Point.canceled -= m_Wrapper.m_BoardActionsCallbackInterface.OnPoint;
                @RightClick.started -= m_Wrapper.m_BoardActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_BoardActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_BoardActionsCallbackInterface.OnRightClick;
                @EscapeMenu.started -= m_Wrapper.m_BoardActionsCallbackInterface.OnEscapeMenu;
                @EscapeMenu.performed -= m_Wrapper.m_BoardActionsCallbackInterface.OnEscapeMenu;
                @EscapeMenu.canceled -= m_Wrapper.m_BoardActionsCallbackInterface.OnEscapeMenu;
            }
            m_Wrapper.m_BoardActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Point.started += instance.OnPoint;
                @Point.performed += instance.OnPoint;
                @Point.canceled += instance.OnPoint;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @EscapeMenu.started += instance.OnEscapeMenu;
                @EscapeMenu.performed += instance.OnEscapeMenu;
                @EscapeMenu.canceled += instance.OnEscapeMenu;
            }
        }
    }
    public BoardActions @Board => new BoardActions(this);

    // Menu
    private readonly InputActionMap m_Menu;
    private IMenuActions m_MenuActionsCallbackInterface;
    private readonly InputAction m_Menu_Click;
    private readonly InputAction m_Menu_EscapeMenu;
    public struct MenuActions
    {
        private @Controls m_Wrapper;
        public MenuActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_Menu_Click;
        public InputAction @EscapeMenu => m_Wrapper.m_Menu_EscapeMenu;
        public InputActionMap Get() { return m_Wrapper.m_Menu; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
        public void SetCallbacks(IMenuActions instance)
        {
            if (m_Wrapper.m_MenuActionsCallbackInterface != null)
            {
                @Click.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnClick;
                @EscapeMenu.started -= m_Wrapper.m_MenuActionsCallbackInterface.OnEscapeMenu;
                @EscapeMenu.performed -= m_Wrapper.m_MenuActionsCallbackInterface.OnEscapeMenu;
                @EscapeMenu.canceled -= m_Wrapper.m_MenuActionsCallbackInterface.OnEscapeMenu;
            }
            m_Wrapper.m_MenuActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @EscapeMenu.started += instance.OnEscapeMenu;
                @EscapeMenu.performed += instance.OnEscapeMenu;
                @EscapeMenu.canceled += instance.OnEscapeMenu;
            }
        }
    }
    public MenuActions @Menu => new MenuActions(this);
    public interface IBoardActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnEscapeMenu(InputAction.CallbackContext context);
    }
    public interface IMenuActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnEscapeMenu(InputAction.CallbackContext context);
    }
}
