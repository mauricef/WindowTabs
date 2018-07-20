#include "StdAfx.h"
#include "platform.h"
#include "TaskList.h"

TaskList::TaskList(LONG_PTR l)
{
	list = l;
	HWND hThumbnailWnd = *(HWND *)(*(LONG_PTR *)(list + DO7X_3264(0xE8, 0x170, 0xEC, 0x178)) + DEF3264(0x30, 0x60));
	lpThumbnailLongPtr = GetWindowLongPtr(hThumbnailWnd, 0);
}

HDPA TaskList::buttonGroups()
{
	LONG_PTR *plp = *(LONG_PTR **)(list + DO7X_3264(0x90, 0xE0, 0x94, 0xE8));
	if(!plp)
	{
		return NULL;
	}
	return (HDPA)plp;
}

// DEF3264: ?_InsertThumbnail@CTaskListThumbnailWnd@@IAEJPAUITaskThumbnail@@PAH@Z
LONG_PTR* TaskList::taskGroup()
{
	return *(LONG_PTR **)(thumbnailsPtr() + DO7X_3264(0x64, 0xA8, 0x74, 0xB8));
}

// DEF3264: ?_RegisterThumbBars@CTaskListThumbnailWnd@@IAEXXZ
LONG_PTR* TaskList::thumbnails()
{
	return *(LONG_PTR **)(thumbnailsPtr() + DO7X_3264(0x68, 0xB0, 0x7C, 0xC8));
}

// DEF3264: ?_RefreshThumbnail@CTaskListThumbnailWnd@@IAEXH@Z
void TaskList::setRedrawFlag()
{
	*(BYTE *)(thumbnailsPtr() + DO7X_3264(0x44, 0x78, 0x54, 0x88)) |= 2;
}


void TaskList::setActiveThumbnail(int index)
{
	int* p = activeThumbnailPtr();
	*p = index;
}

int TaskList::getActiveThumbnail()
{
	return *activeThumbnailPtr();
}


void TaskList::setActiveItem(int index)
{
	int* p = activeItemPtr();
	*p = index;
	InvalidateRect(listHwnd(), NULL, FALSE);
}

int TaskList::getActiveItem()
{
	return *activeItemPtr();
}

//private

LONG_PTR TaskList::thumbnailsPtr() 
{
	return *(LONG_PTR *)(list + DO7X_3264(0xE8, 0x170, 0xEC, 0x178));
}

int* TaskList::activeThumbnailPtr()
{
	return (int *)(lpThumbnailLongPtr + DO7X_3264(0x114, 0x168, 0x128, 0x180));
}

int* TaskList::activeItemPtr()
{
	// Active button
	// DEF3264: ?_SetActiveItem@CTaskListWnd@@IAEXPAUITaskBtnGroup@@H@Z
	return (int *)(list + DO7X_3264(0xB8, 0x128, 0xBC, 0x130));
}

HWND TaskList::listHwnd()
{
	return  *(HWND *)(list + DEF3264(0x04, 0x08));
}