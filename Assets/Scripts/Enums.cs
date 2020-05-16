// mysterious enum dumping ground
public enum DeathType {
    BUMP,
    BISECTED,
    FIRE,
    SQUISHED,
}
public enum EditModeEnum {
    PICKER,
    EDIT,
    OPTIONS
}
public enum EntityTypeEnum {
    BLOCK,
    MOB,
    SPECIALBLOCK,
    PLAYER,
}

public enum EntityPrefabEnum {
    BLOCKPREFAB,
    PLAYERPREFAB,
    SHUFFLEBOTPREFAB,
    PUSHABLEPREFAB,
    FANPREFAB
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
    ENEMY
}

public enum TimeStateEnum {
    NORMAL,
    PAUSED,
    DOUBLE
}