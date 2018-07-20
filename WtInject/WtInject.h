#pragma once

bool InitializeMoveButtonsInGroup();
HDPA GetButtons(LONG_PTR *button_group);
HWND GetButtonWindow(LONG_PTR *button);
BOOL MoveButtonInGroup(LONG_PTR *button_group, int index_from, int index_to);
