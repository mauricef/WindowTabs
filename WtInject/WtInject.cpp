#include "stdafx.h"
#include "platform.h"
#include "TaskBar.h"
#include "WtInject.h"

int nWinVersion = WIN_VERSION_UNSUPPORTED;
TaskBar* taskBar = nullptr;



VS_FIXEDFILEINFO *GetModuleVersionInfo(HMODULE hModule, UINT *puPtrLen);
HWND GetTaskItemWnd(LONG_PTR *task_item);
BOOL TaskbarMoveTaskInGroup(LONG_PTR *task_group, LONG_PTR *task_item_from, LONG_PTR *task_item_to);
void TaskbarMoveTaskInTaskList(TaskList* taskList,
	LONG_PTR *task_group, LONG_PTR *task_item_from, LONG_PTR *task_item_to);
BOOL LLMoveThumbInGroup(TaskList* taskList, int index_from, int index_to);
LONG_PTR *ButtonGroupFromTaskGroup(TaskList* taskList, LONG_PTR *task_group);

int GetWinVersion()
{
	VS_FIXEDFILEINFO *pFixedFileInfo = GetModuleVersionInfo(NULL, NULL);
	int winVersion = WIN_VERSION_UNSUPPORTED;
	if(pFixedFileInfo)
	{
		if(HIWORD(pFixedFileInfo->dwFileVersionMS) == 6) // Major version
		{
			switch(LOWORD(pFixedFileInfo->dwFileVersionMS)) // Minor version
			{
			case 1:
				winVersion = WIN_VERSION_7;
				break;

			case 2:
				winVersion = WIN_VERSION_8;
				break;

			case 3:
				winVersion = WIN_VERSION_81;
				break;
			}
		}
	}
	return winVersion;
}

bool InitializeMoveButtonsInGroup()
{
	
	nWinVersion = GetWinVersion();

	if(nWinVersion == WIN_VERSION_UNSUPPORTED)
		return FALSE;

	taskBar = new TaskBar();
	return taskBar->initialize();
}

HDPA GetButtons(LONG_PTR *button_group)
{
	LONG_PTR *plp = (LONG_PTR *)button_group[DO7X(5, 7)];
	if(!plp)
		return NULL;

	return (HDPA)plp;
}

HWND GetButtonWindow(LONG_PTR *button)
{
	return GetTaskItemWnd((LONG_PTR *)button[DO7X(3, 4)]);
}

BOOL MoveButtonInGroup(LONG_PTR *button_group, int index_from, int index_to)
{
	LONG_PTR *plp;
	int buttons_count;
	LONG_PTR **buttons;
	LONG_PTR *task_group;
	LONG_PTR *task_item_from, *task_item_to;

	task_group = (LONG_PTR *)button_group[DO7X(3, 4)];

	plp = (LONG_PTR *)button_group[DO7X(5, 7)];

	buttons_count = (int)plp[0];
	buttons = (LONG_PTR **)plp[1];

	task_item_from = (LONG_PTR *)buttons[index_from][DO7X(3, 4)];
	task_item_to = (LONG_PTR *)buttons[index_to][DO7X(3, 4)];

	return TaskbarMoveTaskInGroup(task_group, task_item_from, task_item_to);
}

static VS_FIXEDFILEINFO *GetModuleVersionInfo(HMODULE hModule, UINT *puPtrLen)
{
	HRSRC hResource;
	HGLOBAL hGlobal;
	void *pData;
	void *pFixedFileInfo;
	UINT uPtrLen;

	pFixedFileInfo = NULL;
	uPtrLen = 0;

	hResource = FindResource(hModule, MAKEINTRESOURCE(VS_VERSION_INFO), RT_VERSION);
	if(hResource != NULL)
	{
		hGlobal = LoadResource(hModule, hResource);
		if(hGlobal != NULL)
		{
			pData = LockResource(hGlobal);
			if(pData != NULL)
			{
				if(!VerQueryValue(pData, L"\\", &pFixedFileInfo, &uPtrLen) || uPtrLen == 0)
				{
					pFixedFileInfo = NULL;
					uPtrLen = 0;
				}
			}
		}
	}

	if(puPtrLen)
		*puPtrLen = uPtrLen;

	return (VS_FIXEDFILEINFO *)pFixedFileInfo;
}

static HWND GetTaskItemWnd(LONG_PTR *task_item)
{
	LONG_PTR this_ptr;
	LONG_PTR *plp;

	this_ptr = (LONG_PTR)task_item;
	plp = *(LONG_PTR **)this_ptr;

	// CTaskItem::GetWindow(this)
	return ((HWND(__stdcall *)(LONG_PTR))plp[19])(this_ptr);
}

static BOOL TaskbarMoveTaskInGroup(LONG_PTR *task_group, LONG_PTR *task_item_from, LONG_PTR *task_item_to)
{
	LONG_PTR *plp;
	int task_items_count;
	LONG_PTR **task_items;
	int index_from, index_to;
	SECONDARY_TASK_LIST_GET secondary_task_list_get;
	TaskList* secondaryTaskList;
	int i;

	plp = (LONG_PTR *)task_group[4];
	if(!plp)
		return FALSE;

	task_items_count = (int)plp[0];
	task_items = (LONG_PTR **)plp[1];

	for(i = 0; i < task_items_count; i++)
	{
		if(task_items[i] == task_item_from)
		{
			index_from = i;
			break;
		}
	}

	if(i == task_items_count)
		return FALSE;

	for(i = 0; i < task_items_count; i++)
	{
		if(task_items[i] == task_item_to)
		{
			index_to = i;
			break;
		}
	}

	if(i == task_items_count)
		return FALSE;

	plp = task_items[index_from];
	if(index_from < index_to)
		memmove(&task_items[index_from], &task_items[index_from + 1], (index_to - index_from)*sizeof(LONG_PTR *));
	else
		memmove(&task_items[index_to + 1], &task_items[index_to], (index_from - index_to)*sizeof(LONG_PTR *));
	task_items[index_to] = plp;

	TaskbarMoveTaskInTaskList(taskBar->list, task_group, task_item_from, task_item_to);

	secondaryTaskList = taskBar->secondaryTaskListGetFirstLongPtr(&secondary_task_list_get);
	while(secondaryTaskList)
	{
		TaskbarMoveTaskInTaskList(secondaryTaskList, task_group, task_item_from, task_item_to);
		secondaryTaskList = taskBar->secondaryTaskListGetNextLongPtr(&secondary_task_list_get);
	}

	return TRUE;
}

void MoveInButtonGroup(TaskList* taskList, LONG_PTR * button_group, LONG_PTR *task_item_from, LONG_PTR *task_item_to)
{
	int index_from = 0;
	int index_to = 0;
	int i = 0;
	LONG_PTR* plp = (LONG_PTR *)button_group[DO7X(5, 7)];
	HDPA hButtonsDpa = (HDPA)plp;
	int buttons_count = (int)plp[0];
	LONG_PTR** buttons = (LONG_PTR **)plp[1];
	
	for(i = 0; i < buttons_count; i++)
	{
		if((LONG_PTR *)buttons[i][DO7X(3, 4)] == task_item_from)
		{
			index_from = i;
			break;
		}
	}

	if(i < buttons_count)
	{
		for(i = 0; i < buttons_count; i++)
		{
			if((LONG_PTR *)buttons[i][DO7X(3, 4)] == task_item_to)
			{
				index_to = i;
				break;
			}
		}
	}

	if(i < buttons_count)
	{
		plp = (LONG_PTR *)DPA_DeletePtr(hButtonsDpa, index_from);
		if(plp)
		{
			DPA_InsertPtr(hButtonsDpa, index_to, plp);

			int activeItem = taskList->getActiveItem();
			if(activeItem == index_from)
			{
				taskList->setActiveItem(index_to);
			}
			else if(activeItem == index_to)
			{
				taskList->setActiveItem(index_from);
			}
		}
	}
}
void MoveInThumbnails(TaskList* taskList, LONG_PTR *task_item_from, LONG_PTR *task_item_to)
{
	LONG_PTR* plp = taskList->thumbnails();
	int thumbs_count = (int)plp[0];
	LONG_PTR **thumbs = (LONG_PTR **)plp[1];
	int i = 0;
	int index_from = 0;
	int index_to = 0;

	for(i = 0; i < thumbs_count; i++)
	{
		if((LONG_PTR *)thumbs[i][DO7X(2, 3)] == task_item_from)
		{
			index_from = i;
			break;
		}
	}

	if(i < thumbs_count)
	{
		for(i = 0; i < thumbs_count; i++)
		{
			if((LONG_PTR *)thumbs[i][DO7X(2, 3)] == task_item_to)
			{
				index_to = i;
				break;
			}
		}
	}

	if(i < thumbs_count)
	{
		LLMoveThumbInGroup(taskList, index_from, index_to);
	}
}
static void TaskbarMoveTaskInTaskList(TaskList* taskList,
	LONG_PTR *task_group, LONG_PTR *task_item_from, LONG_PTR *task_item_to)
{
	// Taskbar buttons
	LONG_PTR* button_group = ButtonGroupFromTaskGroup(taskList, task_group);
	if(button_group)
	{
		MoveInButtonGroup(taskList, button_group, task_item_from, task_item_to);
	}

	if(taskList->thumbnails() && taskList->taskGroup() == task_group)
	{
		MoveInThumbnails(taskList, task_item_from, task_item_to);
	}
}

static BOOL LLMoveThumbInGroup(TaskList* taskList, int index_from, int index_to)
{
	LONG_PTR *plp;
	int thumbs_count;
	LONG_PTR **thumbs;
	// DEF3264: ?CTaskThumbnail_CreateInstance@@YGJPAUHWND__@@PAUITaskItem@@HHPAUITaskThumbnailHost@@PAPAUITaskThumbnail@@@Z
	size_t nBufferSize = DO7X_3264(0x7C, 0xB0, 0x74, 0xB0) - 2 * sizeof(LONG_PTR);
	BYTE bBuffer[DEF3264(0x7C, 0xB0) - 2 * sizeof(LONG_PTR)]; // max of DO78_3264
	int i;

	// DEF3264: ?_RegisterThumbBars@CTaskListThumbnailWnd@@IAEXXZ
	plp = taskList->thumbnails();
	if(!plp)
		return FALSE;

	thumbs_count = (int)plp[0];
	thumbs = (LONG_PTR **)plp[1];

	memcpy(bBuffer, thumbs[index_from], nBufferSize);
	if(index_from < index_to)
	{
		for(i = index_from; i < index_to; i++)
			memcpy(thumbs[i], thumbs[i + 1], nBufferSize);
	}
	else
	{
		for(i = index_from; i > index_to; i--)
			memcpy(thumbs[i], thumbs[i - 1], nBufferSize);
	}
	memcpy(thumbs[index_to], bBuffer, nBufferSize);

	int activeThumbnail = taskBar->getActiveThumbnail();
	if (activeThumbnail == index_from)
	{
		taskBar->setActiveThumbnail(index_to);
	}
	else if(activeThumbnail == index_to)
	{
		taskBar->setActiveThumbnail(index_from);
	}
	// Redraw flag
	taskList->setRedrawFlag();

	return TRUE;
}

LONG_PTR *ButtonGroupFromTaskGroup(TaskList* taskList, LONG_PTR *task_group)
{
	HDPA hGroups = taskList->buttonGroups();
	if(!hGroups)
	{
		return NULL;
	}

	for(int i = 0; i < DPA_GetPtrCount(hGroups); i++)
	{
		LONG_PTR* button_group = (LONG_PTR *) DPA_GetPtr(hGroups, i);
		if((LONG_PTR *)button_group[DO7X(3, 4)] == task_group)
		{
			return button_group;
		}
	}
	return NULL;
}

void Demo();

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	switch(ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		// NOTE: this is called here only for demonstration purposes.
		// To prevent race conditions, run this code only from the taskbar's UI thread.
		// This can be easily done by subclassing the taskbar and using a custom registered message.
		if(InitializeMoveButtonsInGroup())
		{
			Demo();
		}
		break;

	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}

void Demo()
{
	WCHAR szLogPath[MAX_PATH];
	GetTempPath(MAX_PATH, szLogPath);
	wcscat_s(szLogPath, MAX_PATH, L"taskbar_demo.txt");

	HANDLE hFile = CreateFile(szLogPath, GENERIC_WRITE, FILE_SHARE_READ, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if(hFile == INVALID_HANDLE_VALUE)
	{
		return;
	}

	WCHAR szBuffer[1025];
	DWORD dwWrite, dwBytesWritten;
	BOOL bSwapped = FALSE;

	HDPA hGroups = taskBar->buttonGroups();
	if(hGroups)
	{
		int nGroupsCount = DPA_GetPtrCount(hGroups);

		for(int i = 0; i < nGroupsCount; i++)
		{
			LONG_PTR *button_group = (LONG_PTR *) DPA_GetPtr(hGroups, i);

			dwWrite = wsprintf(szBuffer, L"Button group: %p\r\n", button_group);
			WriteFile(hFile, szBuffer, dwWrite*sizeof(WCHAR), &dwBytesWritten, NULL);

			HDPA hButtons = GetButtons(button_group);
			if(hButtons)
			{
				int nButtonsCount = DPA_GetPtrCount(hButtons);

				for(int j = 0; j < nButtonsCount; j++)
				{
					LONG_PTR *button = (LONG_PTR *) DPA_GetPtr(hButtons, j);

					dwWrite = wsprintf(szBuffer, L"\tButton: %p, HWND: %p\r\n", button, GetButtonWindow(button));
					WriteFile(hFile, szBuffer, dwWrite*sizeof(WCHAR), &dwBytesWritten, NULL);

					if(!bSwapped && j == 1)
					{
						dwWrite = wsprintf(szBuffer, L"\t\tSwapping two of the buttons above...\r\n");
						WriteFile(hFile, szBuffer, dwWrite*sizeof(WCHAR), &dwBytesWritten, NULL);

						BOOL bSuccess = MoveButtonInGroup(button_group, 1, 0);

						dwWrite = wsprintf(szBuffer, L"\t\tDone: %s\r\n", bSuccess ? L"success" : L"failure");
						WriteFile(hFile, szBuffer, dwWrite*sizeof(WCHAR), &dwBytesWritten, NULL);

						bSwapped = TRUE;
					}
				}
			}
		}
	}

	CloseHandle(hFile);

	ShellExecute(NULL, NULL, szLogPath, NULL, NULL, SW_SHOWNORMAL);
}
