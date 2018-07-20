#pragma once
#include "TaskList.h"

typedef struct _secondary_task_list_get {
	int count;
	LONG_PTR *dpa_ptr;
} SECONDARY_TASK_LIST_GET;

class TaskBar
{
public:
	TaskBar(void);
	~TaskBar(void);
	bool initialize(void);
	void setActiveThumbnail(int);
	int getActiveThumbnail();
	HDPA buttonGroups();
	TaskList* secondaryTaskListGetFirstLongPtr(SECONDARY_TASK_LIST_GET *p_secondary_task_list_get);
	TaskList* secondaryTaskListGetNextLongPtr(SECONDARY_TASK_LIST_GET *p_secondary_task_list_get);
	TaskList* list;
	
private:
	LONG_PTR lpTaskSwLongPtr;
};

