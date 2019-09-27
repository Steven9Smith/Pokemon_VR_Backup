using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

////TODO: expose whether pen actually has eraser and which barrel buttons it has

////TODO: hook up pointerId in backend to allow identifying different pens

////REVIEW: have surface distance property to detect how far pen is when hovering?

////REVIEW: does it make sense to have orientation support for pen, too?

namespace UnityEngine.InputSystem.LowLevel
{
    /// <summary>
    /// Default state layout for pen devices.
    /// </summary>
    // IMPORTANT: Must match with PenInputState in native.
    [StructLayout(LayoutKind.Explicit, Size = 36)]
    internal struct PenState : IInputStateTypeInfo
    {
        public static FourCC kFormat => new FourCC('P', 'E', 'N');

        [InputControl(usage = "Point")]
        [FieldOffset(0)]
        public Vector2 position;

        [InputControl(usage = "Secondary2DMotion")]
        [FieldOffset(8)]
        public Vector2 delta;

        [InputControl(layout = "Vector2", usage = "Tilt")]
        [FieldOffset(16)]
        public Vector2 tilt;

        [InputControl(layout = "Analog", usage = "Pressure", defaultState = 0.0f)]
        [FieldOffset(24)]
        public float pressure;

        [InputControl(layout = "Axis", usage = "Twist")]
        [FieldOffset(28)]
        public float twist;

        [InputControl(name = "tip", layout = "Button", bit = (int)PenButton.Tip, usage = "PrimaryAction")]
        [InputControl(name = "press", useStateFrom = "tip", synthetic = true, usages = new string[0])]
        [InputControl(name = "eraser", layout = "Button", bit = (int)PenButton.Eraser)]
        [InputControl(name = "inRange", layout = "Button", bit = (int)PenButton.InRange, synthetic = true)]
        [InputControl(name = "barrel1", layout = "Button", bit = (int)PenButton.BarrelFirst, alias = "barrelFirst", usage = "SecondaryAction")]
        [InputControl(name = "barrel2", layout = "Button", bit = (int)PenButton.BarrelSecond, alias = "barrelSecond")]
        [InputControl(name = "barrel3", layout = "Button", bit = (int)PenButton.BarrelThird, alias = "barrelThird")]
        [InputControl(name = "barrel4", layout = "Button", bit = (int)PenButton.BarrelFourth, alias = "barrelFourth")]
        // "Park" unused controls.
        [InputControl(name = "radius", layout = "Vector2", format = "VEC2", sizeInBits = 64, usage = "Radius", offset = InputStateBlock.AutomaticOffset)]
        [InputControl(name = "pointerId", layout = "Digital", format = "UINT", sizeInBits = 32, offset = InputStateBlock.AutomaticOffset)] ////TODO: this should be used
        [FieldOffset(32)]
        public ushort buttons;

        // Not currently used, but still needed in this struct for padding,
        // as il2cpp does not implement FieldOffset.
        [FieldOffset(34)]
        ushort displayIndex;

        public PenState WithButton(PenButton button, bool state = true)
        {
            if (state)
                buttons |= (ushort)(1 << (int)button);
            else
                buttons &= (ushort)~(1 << (int)button);
            return this;
        }

        public FourCC format
        {
            get { return kFormat; }
        }
    }
}

namespace UnityEngine.InputSystem
{
    /// <summary>
    /// Enumeration of buttons on a <see cref="Pen"/>.
    /// </summary>
    public enum PenButton
    {
        /// <summary>
        /// Button at the tip of a pen.
        /// </summary>
        /// <seealso cref="Pen.tip"/>
        Tip,

        /// <summary>
        /// Button located end of pen opposite to <see cref="Tip"/>.
        /// </summary>
        /// <remarks>
        /// Pens do not necessarily have an eraser. If a pen doesn't, the respective button
        /// does nothing and will always be unpressed.
        /// </remarks>
        /// <seealso cref="Pen.eraser"/>
        Eraser,

        /// <summary>
        /// First button on the side of the pen.
        /// </summary>
        /// <see cref="Pen.firstBarrelButton"/>
        BarrelFirst,

        /// <summary>
        /// Second button on the side of the pen.
        /// </summary>
        /// <seealso cref="Pen.secondBarrelButton"/>
        BarrelSecond,

        /// <summary>
        /// Artificial button that indicates whether the pen is in detection range or not.
        /// </summary>
        /// <remarks>
        /// Range detection may not be supported by a pen/tablet.
        /// </remarks>
        /// <seealso cref="Pen.inRange"/>
        InRange,

        /// <summary>
        /// Third button on the side of the pen.
        /// </summary>
        /// <seealso cref="Pen.thirdBarrelButton"/>
        BarrelThird,

        /// <summary>
        /// Fourth button on the side of the pen.
        /// </summary>
        /// <see cref="Pen.fourthBarrelButton"/>
        BarrelFourth,

        /// <summary>
        /// Synonym for <see cref="BarrelFirst"/>.
        /// </summary>
        Barrel1 = BarrelFirst,

        /// <summary>
        /// Synonym for <see cref="BarrelSecond"/>.
        /// </summary>
        Barrel2 = BarrelSecond,

        /// <summary>
        /// Synonym for <see cref="BarrelThird"/>.
        /// </summary>
        Barrel3 = BarrelThird,

        /// <summary>
        /// Synonym for <see cref="BarrelFourth"/>.
        /// </summary>
        Barrel4 = BarrelFourth,
    }

    /// <summary>
    /// A pen/stylus input device.
    /// </summary>
    /// <remarks>
    /// Unlike mice but like touch, pens are absolute pointing devices moving across a fixed
    /// surface area.
    ///
    /// The <see cref="tip"/> acts as a button that is considered pressed as long as the pen is in contact with the
    /// tablet surface.
    /// </remarks>
    [InputControlLayout(stateType = typeof(PenState), isGenericTypeOfDevice = true)]
    [Scripting.Preserve]
    public class Pen : Pointer
    {
        ////TODO: give the tip and eraser a very low press point
        /// <summary>
        /// The tip button of the pen.
        /// </summary>
        /// <seealso cref="PenButton.Tip"/>
        public ButtonControl tip { get; private set; }

        /// <summary>
        /// The eraser button of the pen, i.e. the button on the end opposite to the tip.
        /// </summary>
        /// <remarks>
        /// If the pen does not have an eraser button, this control will still be present
        /// but will not trigger.
        /// </remarks>
        /// <seealso cref="PenButton.Eraser"/>
        public ButtonControl eraser { get; private set; }

        /// <summary>
        /// The button on the side of the pen barrel and located closer to the tip of the pen.
        /// </summary>
        /// <remarks>
        /// If the pen does not have barrel buttons, this control will still be present
        /// but will not trigger.
        /// </remarks>
        /// <seealso cref="PenButton.BarrelFirst"/>
        public ButtonControl firstBarrelButton { get; private set; }

        /// <summary>
        /// The button on the side of the pen barrel and located closer to the eraser end of the pen.
        /// </summary>
        /// <remarks>
        /// If the pen does not have barrel buttons, this control will still be present
        /// but will not trigger.
        /// </remarks>
        /// <seealso cref="PenButton.BarrelSecond"/>
        public ButtonControl secondBarrelButton { get; private set; }

        public ButtonControl thirdBarrelButton { get; private set; }

        public ButtonControl fourthBarrelButton { get; private set; }

        /// <summary>
        /// Button control that indicates whether the pen is in range of the tablet surface or not.
        /// </summary>
        /// <remarks>
        /// This is a synthetic control (<see cref="InputControl.synthetic"/>).
        ///
        /// If range detection is not supported by the pen, this button will always be "pressed".
        /// </remarks>
        /// <seealso cref="PenButton.InRange"/>
        public ButtonControl inRange { get; private set; }

        public Vector2Control tilt { get; private set; }

        /// <summary>
        /// Rotation of the pointer around its own axis. 0 means the pointer is facing away from the user (12 'o clock position)
        /// and ~1 means the pointer has been rotated clockwise almost one full rotation.
        /// </summary>
        /// <remarks>
        /// Twist is generally only supported by pens and even among pens, twist support is rare. An example product that
        /// supports twist is the Wacom Art Pen.
        ///
        /// The axis of rotation is the vector facing away from the pointer surface when the pointer is facing straight up
        /// (i.e. the surface normal of the pointer surface). When the pointer is tilted, the rotation axis is tilted along
        /// with it.
        /// </remarks>
        public AxisControl twist { get; private set; }

        /// <summary>
        /// The pen that was active or connected last or <c>null</c> if there is no pen.
        /// </summary>
        public new static Pen current { get; internal set; }

        public ButtonControl this[PenButton button]
        {
            get
            {
                switch (button)
                {
                    case PenButton.Tip: return tip;
                    case PenButton.Eraser: return eraser;
                    case PenButton.BarrelFirst: return firstBarrelButton;
                    case PenButton.BarrelSecond: return secondBarrelButton;
                    case PenButton.BarrelThird: return thirdBarrelButton;
                    case PenButton.BarrelFourth: return fourthBarrelButton;
                    case PenButton.InRange: return inRange;
                    default:
                        throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(PenButton));
                }
            }
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this)
                current = null;
        }

        protected override void FinishSetup()
        {
            tip = GetChildControl<ButtonControl>("tip");
            eraser = GetChildControl<ButtonControl>("eraser");
            firstBarrelButton = GetChildControl<ButtonControl>("barrel1");
            secondBarrelButton = GetChildControl<ButtonControl>("barrel2");
            thirdBarrelButton = GetChildControl<ButtonControl>("barrel3");
            fourthBarrelButton = GetChildControl<ButtonControl>("barrel4");
            inRange = GetChildControl<ButtonControl>("inRange");
            tilt = GetChildControl<Vector2Control>("tilt");
            twist = GetChildControl<AxisControl>("twist");
            base.FinishSetup();
        }
    }
}
