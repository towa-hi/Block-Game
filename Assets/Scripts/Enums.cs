// mysterious enum dumping ground

using System.Diagnostics.CodeAnalysis;

public enum CursorModeEnum {
    POINTING,
    PLACING,
    SELECTING,
    HOLDING,
    GAME,
    OFF,
}

public enum DeathType {
    BUMP,
    BISECTED,
    FIRE,
    SQUISHED,
}
public enum EditModeEnum {
    PICKER,
    EDIT,
    OPTIONS,
    BGPICKER,
    BGEDIT,
}

public enum EditTabEnum {
    PICKER,
    EDIT,
    OPTIONS,
}

public enum EntityTypeEnum {
    BG,
    BLOCK,
    MOB,
    SPECIALBLOCK,
    PLAYER,
}

public enum GameModeEnum {
    PLAYING,
    EDITING,
    PLAYTESTING,
}

public enum LocomotionStateEnum {
    READY,
    WAITING,
    WALKING,
    TURNING,
    JUMPING,
    FALLING,
    HOPPING,
    RISING,
    FLOATING,
}

public enum MouseStateEnum {
    DEFAULT,
    CLICKED,
    HELD,
    RELEASED,
}

public enum MoveTypeEnum {
    INANIMATE,
    PATROL,
    FLY,
    PATHPATROL,
    PATHFLY,
}

public enum PlayStateEnum {
    INITIALIZATION,
    DIALOGUE,
    PLAYING,
    LOST,
    WON,
    MENU,
}

public enum PlacementStateEnum {
    DEFAULT,
    CLICKED,
    HELD,
    UNCLICKED,
    CANCELLED,
}

public enum SelectionStateEnum {
    UNSELECTED,
    SELECTED,
    INVALID,
}

public enum TeamEnum {
    PLAYER,
    NEUTRAL,
    ENEMY,
}

public enum TimeStateEnum {
    NORMAL,
    PAUSED,
    DOUBLE,
}