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
    public struct BoardActions
    {
        private @Controls m_Wrapper;
        public BoardActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_Board_Click;
        public InputAction @Point => m_Wrapper.m_Board_Point;
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
            }
        }
    }
    public BoardActions @Board => new BoardActions(this);
    public interface IBoardActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
    }
}
