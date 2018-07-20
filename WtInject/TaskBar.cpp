#include "StdAfx.h"
#include "TaskBar.h"
#include "platform.h"

TaskBar::TaskBar(void)
{

}

bool TaskBar::initialize(void)
{
	HWND hTaskbarWnd = FindWindow(L"Shell_TrayWnd", NULL);
	if(!hTaskbarWnd)
		return false;

	HWND hTaskSwWnd = (HWND)GetProp(hTaskbarWnd, L"TaskbandHWND");
	if(!hTaskSwWnd)
		return false;

	lpTaskSwLongPtr = GetWindowLongPtr(hTaskSwWnd, 0);

	HWND hTaskListWnd = FindWindowEx(hTaskSwWnd, NULL, L"MSTaskListWClass", NULL);
	if(!hTaskListWnd)
		return false;

	list = new TaskList(GetWindowLongPtr(hTaskListWnd, 0));

	return true;
}

void TaskBar::setActiveThumbnail(int index)
{
	list->setActiveThumbnail(index);
}

int TaskBar::getActiveThumbnail()
{
	return list->getActiveThumbnail();
}

HDPA TaskBar::buttonGroups()
{
	return list->buttonGroups();
}

TaskList* TaskBar::secondaryTaskListGetFirstLongPtr(SECONDARY_TASK_LIST_GET *p_secondary_task_list_get)
{
	LONG_PTR lp;
	int nSecondaryTaskbarsCount;
	LONG_PTR *dpa_ptr;
	LONG_PTR lpSecondaryTaskListLongPtr;

	if(nWinVersion >= WIN_VERSION_8)
	{
		lp = *(LONG_PTR *)(lpTaskSwLongPtr + DOX81_3264(0x8C, 0xF0, 0x90, 0xF8));

		// DEF3264: CTaskListWndMulti::ActivateTask
		nSecondaryTaskbarsCount = *(int *)(lp + DEF3264(0x14, 0x28));
		if(nSecondaryTaskbarsCount > 0)
		{
			dpa_ptr = (*(LONG_PTR ***)(lp + DEF3264(0x10, 0x20)))[1];

			lpSecondaryTaskListLongPtr = *dpa_ptr;
			lpSecondaryTaskListLongPtr -= DEF3264(0x14, 0x28);

			p_secondary_task_list_get->count = nSecondaryTaskbarsCount - 1;
			p_secondary_task_list_get->dpa_ptr = dpa_ptr + 1;

			return new TaskList(lpSecondaryTaskListLongPtr);
		}
	}

	return 0;
}

TaskList* TaskBar::secondaryTaskListGetNextLongPtr(SECONDARY_TASK_LIST_GET *p_secondary_task_list_get)
{
	LONG_PTR lpSecondaryTaskListLongPtr;

	if(p_secondary_task_list_get->count > 0)
	{
		lpSecondaryTaskListLongPtr = *p_secondary_task_list_get->dpa_ptr;
		lpSecondaryTaskListLongPtr -= DEF3264(0x14, 0x28);

		p_secondary_task_list_get->count--;
		p_secondary_task_list_get->dpa_ptr++;

		return new TaskList(lpSecondaryTaskListLongPtr);
	}

	return NULL;
}


TaskBar::~TaskBar(void)
{
}


