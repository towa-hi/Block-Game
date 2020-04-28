// mysterious enum dumping ground

public enum EditModeEnum {
    PLACEENTITY,
    EDITENTITY,
    DELETEENTITY
}
public enum EntityTypeEnum {
    BLOCK,
    MOB,
    SPECIALBLOCK,
}

public enum GameModeEnum {
    PLAYING,
    EDITING
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

public enum TimeStateEnum {
    NORMAL,
    PAUSED,
    DOUBLE
}