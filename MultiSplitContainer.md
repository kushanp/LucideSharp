# MultiSplitContainer

A reusable **.NET 8 WinForms** custom control that hosts multiple resizable panels separated by draggable splitter bars. Each splitter bar has three buttons (collapse before, restore, collapse after). The control supports the Visual Studio WinForms designer.

**Hard constraint:** does **not** use `System.Windows.Forms.SplitContainer`.

---

## Table of contents

1. [Solution layout](#solution-layout)
2. [How to recreate the solution](#how-to-recreate-the-solution)
3. [Architecture overview](#architecture-overview)
4. [Control library (`MultiSplitContainer.Controls`)](#control-library-multisplitcontainercontrols)
5. [Layout engine](#layout-engine)
6. [Splitter rendering and interaction](#splitter-rendering-and-interaction)
7. [Collapse and restore](#collapse-and-restore)
8. [Designer support](#designer-support)
9. [Demo application](#demo-application)
10. [Public API reference](#public-api-reference)
11. [Build and run](#build-and-run)
12. [Designer limitations](#designer-limitations)

---

## Solution layout

```
MultiSplitContainer/
├── MultiSplitContainer.slnx
├── README.md
└── src/
    ├── MultiSplitContainer.Controls/          # Reusable class library
    │   ├── MultiSplitContainer.Controls.csproj
    │   ├── MultiSplitContainer.cs               # Main container control
    │   ├── MultiSplitPanel.cs                   # Panel subclass
    │   ├── MultiSplitPanelCollection.cs         # Designer-serializable collection
    │   ├── MultiSplitContainerEvents.cs         # Event argument types
    │   ├── SplitterButtonKind.cs                # Internal button enum
    │   ├── SplitterHitTestResult.cs             # Internal hit-test struct
    │   └── Design/
    │       ├── MultiSplitContainerDesigner.cs     # ParentControlDesigner
    │       └── MultiSplitContainerActionList.cs   # Smart-tag actions
    └── MultiSplitContainer.Demo/                # WinForms demo app
        ├── MultiSplitContainer.Demo.csproj
        ├── Program.cs
        ├── MainForm.cs
        └── MainForm.Designer.cs
```

---

## How to recreate the solution

### Requirements

- Windows
- .NET 8 SDK
- Visual Studio 2022+ (optional, for designer)

### 1. Create projects

```bash
mkdir MultiSplitContainer && cd MultiSplitContainer

dotnet new sln -n MultiSplitContainer
dotnet new classlib -n MultiSplitContainer.Controls -o src/MultiSplitContainer.Controls -f net8.0
dotnet new winforms -n MultiSplitContainer.Demo -o src/MultiSplitContainer.Demo -f net8.0

dotnet sln add src/MultiSplitContainer.Controls/MultiSplitContainer.Controls.csproj
dotnet sln add src/MultiSplitContainer.Demo/MultiSplitContainer.Demo.csproj
dotnet add src/MultiSplitContainer.Demo reference src/MultiSplitContainer.Controls
```

### 2. Configure project files

**`src/MultiSplitContainer.Controls/MultiSplitContainer.Controls.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>MultiSplitContainer.Controls</RootNamespace>
    <AssemblyName>MultiSplitContainer.Controls</AssemblyName>
  </PropertyGroup>
</Project>
```

**`src/MultiSplitContainer.Demo/MultiSplitContainer.Demo.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>MultiSplitContainer.Demo</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MultiSplitContainer.Controls\MultiSplitContainer.Controls.csproj" />
  </ItemGroup>
</Project>
```

No extra NuGet packages are required. Designer types (`System.Windows.Forms.Design`, `System.ComponentModel.Design`) ship with the .NET 8 Windows targeting pack.

### 3. Implement source files

Create every `.cs` file listed in [Solution layout](#solution-layout). The sections below describe exactly what each file must contain and how the pieces interact.

---

## Architecture overview

```
┌─────────────────────────────────────────────────────────────┐
│  MultiSplitContainer : ContainerControl                     │
│  ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐                │
│  │ Panel 0  │ │ │ Panel 1  │ │ │ Panel 2  │  ...           │
│  │ (child   │ │ │ (child   │ │ │ (child   │                │
│  │  control)│ │ │  control)│ │ │  control)│                │
│  └──────────┘ │ └──────────┘ │ └──────────┘                │
│               ↑               ↑                             │
│         splitter 0      splitter 1   (drawn in OnPaint)     │
└─────────────────────────────────────────────────────────────┘
```

| Layer | Responsibility |
|---|---|
| `MultiSplitPanel` | Hosts child controls; stores `SplitSize`, `Collapsed`, minimums |
| `MultiSplitPanelCollection` | Designer-serializable `IList`; delegates add/remove to container |
| `MultiSplitContainer` | Manual layout, splitter painting, mouse handling, events |
| `MultiSplitContainerDesigner` | Panel selection, smart-tag actions, design-time mutations |
| `SplitterButtonKind` / `SplitterHitTestResult` | Internal hit-test model |

**Design principles:**

- Panels are real child `Control` instances (required for designer serialization and drag-drop of child controls).
- Panel bounds are assigned manually via `SetBounds` — no `Dock`-based nested containers.
- Splitter bars are **not** separate controls; they are drawn in `OnPaint` and hit-tested in mouse handlers.
- Splitter index `i` is the bar **between** `Panels[i]` and `Panels[i+1]`.
- Double-buffering via `ControlStyles.OptimizedDoubleBuffer | AllPaintingInWmPaint | UserPaint`.

---

## Control library (`MultiSplitContainer.Controls`)

### `SplitterButtonKind.cs` (internal enum)

```csharp
internal enum SplitterButtonKind { None, CollapseBefore, CollapseAfter, Restore }
```

### `SplitterHitTestResult.cs` (internal readonly struct)

Fields: `SplitterIndex`, `ButtonKind`, `IsDragRegion`. Static `None` sentinel. `IsSplitter` => index >= 0.

### `MultiSplitContainerEvents.cs`

Four event argument types:

| Type | Properties |
|---|---|
| `SplitterMovingEventArgs` | `SplitterIndex`, `Delta` |
| `SplitterMovedEventArgs` | `SplitterIndex`, `BeforePanel`, `AfterPanel` |
| `PanelCollapsedEventArgs` | `PanelIndex`, `Panel` |
| `PanelRestoredEventArgs` | `PanelIndex`, `Panel` |

### `MultiSplitPanel.cs`

`MultiSplitPanel : Panel` with `[ToolboxItem(false)]`.

| Property | Default | Notes |
|---|---|---|
| `Collapsed` | `false` | Notifies owner unless `SuppressOwnerNotification` is set |
| `CollapsedSize` | `0` | Pixel size along split axis when collapsed |
| `SplitSize` | `100` | Preferred size along split axis |
| `LastNonCollapsedSplitSize` | hidden | Saved before collapse; used by restore |
| `MinimumSplitSize` | `25` | Maps to `MinimumSize.Width` (vertical) or `.Height` (horizontal) |
| `Dock` / `Anchor` | hidden | Always forced to `None` by container |

Internal members: `Owner`, `SuppressOwnerNotification`, `RememberSplitSize()`, `GetMinimumSplitSize(Orientation)`.

### `MultiSplitPanelCollection.cs`

Implements non-generic `IList` (required for WinForms designer serialization).

- All `Add`/`Insert`/`Remove` operations delegate to `MultiSplitContainer` methods.
- Internal `*Internal` methods mutate the backing `List<MultiSplitPanel>` without re-entering container logic.
- Indexed set throws `NotSupportedException`.

Mark the container's `Panels` property:

```csharp
[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
public MultiSplitPanelCollection Panels => _panels;
```

### `MultiSplitContainer.cs`

`MultiSplitContainer : ContainerControl` with attributes:

```csharp
[Designer(typeof(MultiSplitContainerDesigner))]
[DefaultProperty(nameof(Panels))]
[DefaultEvent(nameof(SplitterMoved))]
```

#### Appearance defaults

| Property | Default value |
|---|---|
| `Orientation` | `Vertical` |
| `SplitterWidth` | `7` (minimum clamp: `4`) |
| `SplitterBackColor` | `RGB(236, 236, 236)` |
| `SplitterHoverBackColor` | `RGB(210, 228, 250)` |
| `SplitterButtonBackColor` | `RGB(245, 245, 245)` |
| `SplitterButtonHoverBackColor` | `RGB(200, 220, 252)` |
| `SplitterButtonBorderColor` | `RGB(160, 160, 160)` |
| `SplitterButtonPressedBackColor` | `RGB(0, 120, 215)` (internal, used when pressed) |

#### Public methods

```csharp
void AddPanel();
void InsertPanel(int index);
void RemovePanelAt(int index);          // no-op if only 1 panel remains
void CollapsePanel(int panelIndex);
void RestorePanel(int panelIndex);
void RestoreAll();
```

#### Panel lifecycle

**Insert** (`InsertPanel(index, panel)`):
1. Set `panel.Owner = this`, `Dock = None`, `Anchor = None`, `TabStop = false`
2. Add to `_panels` list and `Controls` collection
3. Default `SplitSize` if <= 0
4. `NormalizePanelSizes()` → `PerformLayout()` → `Invalidate()`

**Remove** (`RemovePanelAt`):
1. Unhook owner, remove from list and `Controls`, dispose panel
2. Relayout

**EnsureMinimumPanels** (called in `OnHandleCreated` when count is 0): adds 2 default panels. Designer-serialized forms skip this because panels are already present.

#### Internal state tracked per interaction

```csharp
List<Rectangle> _splitterRects;
List<Rectangle[]> _buttonRects;   // 3 buttons per splitter
int _hoveredSplitterIndex, _hoveredButton;
int _pressedSplitterIndex, _pressedButton;
int _dragSplitterIndex, _dragStartCoordinate, _dragStartBeforeSize, _dragStartAfterSize;
bool _layoutSuspended;            // true during live drag
```

---

## Layout engine

### Coordinate model

| Orientation | Primary axis (split sizes) | Secondary axis (fills client) |
|---|---|---|
| `Vertical` | Width (left → right) | Height |
| `Horizontal` | Height (top → bottom) | Width |

Helper methods: `GetPrimaryClientSize()`, `GetSecondaryClientSize()`, `GetPrimaryCoordinate(Point)`.

### `ApplyLayout(updateSplitSizes)`

1. Clear `_splitterRects` and `_buttonRects`
2. `available = primaryClientSize - (panelCount - 1) * SplitterWidth`
3. `sizes = CalculatePanelSizes(available)`
4. Walk panels left-to-right / top-to-bottom:
   - `SetBounds` on each panel
   - Record splitter rect and button rects after each panel (except last)
5. If `updateSplitSizes`: write computed sizes back to non-collapsed panels' `SplitSize` and call `RememberSplitSize()`

During live drag, `updateSplitSizes` is `false` until mouse-up.

### `CalculatePanelSizes(available)`

1. Collapsed panels get `CollapsedSize`; sum their total
2. `flexibleAvailable = available - collapsedTotal`
3. Non-collapsed panels receive space **proportionally** to their `SplitSize`:
   ```
   size[i] = max(minimum[i], splitSize[i] * flexibleAvailable / flexibleTotal)
   ```
4. Remainder pixels go to the last flexible panel
5. `EnforceMinimumSizes()` iteratively borrows space from other flexible panels if any are below minimum

When parent resizes, proportional distribution keeps relative panel ratios stable.

### Drag resize (`ApplySplitterDrag`)

Only affects `Panels[splitterIndex]` and `Panels[splitterIndex + 1]`:

```
beforeSize = dragStartBeforeSize + delta
afterSize  = dragStartAfterSize - delta
```

Clamp to each panel's minimum (or `CollapsedSize` if collapsed). Update `SplitSize` on non-collapsed panels. Fire `SplitterMoving` during drag; `SplitterMoved` on finalize.

---

## Splitter rendering and interaction

### Button sizing (proportional to `SplitterWidth`)

```csharp
int GetButtonSize()    => max(4, SplitterWidth - max(1, SplitterWidth/8) * 2);
int GetButtonSpacing() => max(1, SplitterWidth / 6);
```

Examples:
- `SplitterWidth = 7`  → button ~5px, spacing 1px
- `SplitterWidth = 24` → button ~20px, spacing 4px

Buttons are square. Layout:
- **Vertical** orientation: 3 buttons stacked vertically, centered in splitter bar
- **Horizontal** orientation: 3 buttons in a horizontal row, centered in splitter bar

### Button glyphs

Drawn in `DrawButtonGlyph` with pen width and arrow extent scaled to `min(bounds.Width, bounds.Height)`:

| Button | Vertical glyph | Horizontal glyph |
|---|---|---|
| `CollapseBefore` | Arrow pointing left | Arrow pointing up |
| `CollapseAfter` | Arrow pointing right | Arrow pointing down |
| `Restore` | Double arrow (left + right) | Double arrow (up + down) |

### Visual feedback

- Splitter bar: `SplitterBackColor` normally, `SplitterHoverBackColor` when hovered (drag region only)
- Buttons: normal / hover / pressed colors; pressed uses white glyph on blue background
- Grip dots drawn at splitter center
- Cursor: `Cursors.VSplit` or `Cursors.HSplit` over drag region

### Hit testing (`HitTest(Point)`)

Priority order:
1. Button rectangles (3 per splitter) → returns button kind
2. Splitter rectangle (excluding buttons) → returns `IsDragRegion = true`
3. Otherwise → `None`

### Mouse flow

| Event | Action |
|---|---|
| `MouseMove` (dragging) | `ApplySplitterDrag(..., finalize: false)` |
| `MouseMove` (idle) | Update hover state, set cursor |
| `MouseDown` (button) | Record pressed splitter + button |
| `MouseDown` (drag region) | Start drag, `Capture = true` |
| `MouseUp` (button) | `HandleButtonClick` if still over same button |
| `MouseUp` (drag) | Finalize drag, release capture |
| `MouseLeave` | Clear hover (unless dragging) |

---

## Collapse and restore

### Collapse (single panel)

`CollapsePanelInternal(panel, index)`:
1. `panel.RememberSplitSize()`
2. `SetPanelCollapsed(panel, true)` — uses `SuppressOwnerNotification` to avoid recursion
3. `panel.SplitSize = panel.CollapsedSize`
4. Relayout, fire `PanelCollapsed`

### Collapse via splitter buttons

On splitter `i`:
- **CollapseBefore** → collapse `Panels[i]`
- **CollapseAfter** → collapse `Panels[i + 1]`

### Restore via splitter button

`RestoreAdjacentPanels(splitterIndex, before, after)`:
- If both collapsed: restore each to `LastNonCollapsedSplitSize` or half of combined available space
- If one collapsed: restore that one to saved size or half available
- If neither collapsed: no-op

### Restore (API)

- `RestorePanel(index)`: sets `Collapsed = false`, `SplitSize = LastNonCollapsedSplitSize` (or default), fires `PanelRestored`
- `RestoreAll()`: restores every collapsed panel, single relayout at end

Collapsed panels keep `SplitterWidth`-sized bars visible between them and neighbors.

---

## Designer support

### `MultiSplitContainerDesigner : ParentControlDesigner`

- `AutoResizeHandles = true`
- `EnableDragDrop(true)`
- `CanParent(control)` → `true` only for `MultiSplitPanel`
- `GetHitTest(point)` → returns `true` when point is inside a panel bounds (allows selecting inner panels in designer)
- Smart-tag mutations call `RaiseComponentChanging` / `RaiseComponentChanged` on the relevant `MemberDescriptor`

### `MultiSplitContainerActionList`

| Smart-tag item | Action |
|---|---|
| Orientation | Property grid binding |
| Add Panel | `container.AddPanel()` |
| Remove Last Panel | `container.RemovePanelAt(count - 1)` (if count > 1) |
| Toggle Orientation | Flip `Vertical` ↔ `Horizontal` |

### Designer workflow

1. Drop `MultiSplitContainer` on a form
2. Use **Panels** collection or smart tags to add/remove panels
3. Click inside a panel to select it
4. Drop standard controls onto the selected panel
5. Panel child controls serialize into the `.Designer.cs` file

---

## Demo application

### `Program.cs`

Standard .NET 8 WinForms entry point with `[STAThread]` and `ApplicationConfiguration.Initialize()`.

### `MainForm` layout

| UI element | Purpose |
|---|---|
| `MenuStrip` → Layout → Toggle Orientation | Flips `multiSplitContainer1.Orientation` |
| `MenuStrip` → Panels → Add Panel | Calls `AddPanel()`, applies color rotation |
| `MenuStrip` → Panels → Remove Last Panel | Calls `RemovePanelAt(count - 1)` |
| `MenuStrip` → Panels → Collapse Next Panel | Cycles through `CollapsePanel(i)` |
| `MenuStrip` → Panels → Restore All | Calls `RestoreAll()` |
| `StatusStrip` | Shows feedback from control events and menu actions |

### `MultiSplitContainer` demo settings

```csharp
multiSplitContainer1.Dock = DockStyle.Fill;
multiSplitContainer1.Orientation = Orientation.Vertical;
multiSplitContainer1.SplitterWidth = 24;
```

### Four panels

| Panel | BackColor | SplitSize | Child controls |
|---|---|---|---|
| `panel1` | `RGB(214, 234, 248)` | 220 | Label + `TreeView` (solution tree) |
| `panel2` | `RGB(225, 245, 225)` | 260 | Label + `PropertyGrid` (bound to `multiSplitContainer1`) |
| `panel3` | `RGB(255, 244, 214)` | 320 | Label + `RichTextBox` (usage instructions) |
| `panel4` | `RGB(245, 225, 245)` | 280 | Label + `DataGridView` (sample panel data) |

Each panel uses a docked header `Label` (24px top) and a `Dock = Fill` content control.

### Namespace collision workaround

The demo namespace `MultiSplitContainer.Demo` conflicts with the type name `MultiSplitContainer`. In `MainForm.Designer.cs` use type aliases:

```csharp
using SplitContainerControl = MultiSplitContainer.Controls.MultiSplitContainer;
using SplitPanelControl = MultiSplitContainer.Controls.MultiSplitPanel;
```

`MainForm.cs` can import `MultiSplitContainer.Controls` directly because it references the field `multiSplitContainer1` (lowercase) without ambiguity.

### Event wiring (`MainForm.cs`)

Subscribe to `SplitterMoved`, `PanelCollapsed`, `PanelRestored` and update the status label.

`DecoratePanels()` assigns rotating background colors from a 6-color palette when panels are added at runtime.

---

## Public API reference

### `MultiSplitContainer`

```csharp
public class MultiSplitContainer : ContainerControl
{
    // Properties
    public Orientation Orientation { get; set; }
    public int SplitterWidth { get; set; }
    public Color SplitterBackColor { get; set; }
    public Color SplitterHoverBackColor { get; set; }
    public Color SplitterButtonBackColor { get; set; }
    public Color SplitterButtonHoverBackColor { get; set; }
    public Color SplitterButtonBorderColor { get; set; }
    public MultiSplitPanelCollection Panels { get; }

    // Methods
    public void AddPanel();
    public void InsertPanel(int index);
    public void RemovePanelAt(int index);
    public void CollapsePanel(int panelIndex);
    public void RestorePanel(int panelIndex);
    public void RestoreAll();

    // Events
    public event EventHandler<SplitterMovingEventArgs>? SplitterMoving;
    public event EventHandler<SplitterMovedEventArgs>? SplitterMoved;
    public event EventHandler<PanelCollapsedEventArgs>? PanelCollapsed;
    public event EventHandler<PanelRestoredEventArgs>? PanelRestored;
}
```

### `MultiSplitPanel`

```csharp
public class MultiSplitPanel : Panel
{
    public bool Collapsed { get; set; }
    public int CollapsedSize { get; set; }
    public int SplitSize { get; set; }
    public int MinimumSplitSize { get; set; }
    // LastNonCollapsedSplitSize is internal/hidden
}
```

---

## Build and run

```bash
dotnet build MultiSplitContainer.slnx
dotnet run --project src/MultiSplitContainer.Demo/MultiSplitContainer.Demo.csproj
```

Open `MultiSplitContainer.slnx` in Visual Studio 2022+ and set **MultiSplitContainer.Demo** as the startup project.

Expected result: a 1100×650 form with four colored panels, 24px splitters with proportionally sized buttons, a menu bar, and a status bar. Splitters drag smoothly; buttons collapse/restore adjacent panels.

---

## Designer limitations

| Topic | Behavior |
|---|---|
| Splitter interaction | Runtime only — splitters are painted, not designer controls |
| Panel selection | Click inside panel area (designer forwards hit-test to panels) |
| Minimum panels | At least one panel must remain; runtime auto-creates 2 if none exist |
| Namespace collision | Qualify control type when app namespace starts with `MultiSplitContainer` |
| `System.Design` NuGet | Not needed — designer APIs come from the `net8.0-windows` targeting pack |

---

## Acceptance checklist

- [x] Solution builds with 0 errors and 0 warnings
- [x] Does not reference `System.Windows.Forms.SplitContainer`
- [x] Multiple panels with vertical and horizontal orientation
- [x] Draggable splitters respecting minimum sizes
- [x] Three buttons per splitter (collapse before, restore, collapse after)
- [x] Collapse/restore affects only adjacent panels
- [x] Proportional layout on parent resize
- [x] Designer serialization of panels and child controls
- [x] Smart-tag: Add Panel, Remove Panel, Toggle Orientation
- [x] Demo with 4 panels, menu commands, and event feedback
- [x] Splitter buttons scale proportionally with `SplitterWidth`