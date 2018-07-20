#pragma once

class TaskList
{
public:
	TaskList(LONG_PTR);
	~TaskList(void);
	HDPA buttonGroups();
	void setActiveThumbnail(int);
	int getActiveThumbnail();
	void setActiveItem(int);
	int getActiveItem();
	LONG_PTR* taskGroup();
	LONG_PTR* thumbnails();
	void setRedrawFlag();

private:
	LONG_PTR list;
	LONG_PTR lpThumbnailLongPtr;
	int* activeThumbnailPtr();
	int* activeItemPtr();
	HWND TaskList::listHwnd();
	LONG_PTR thumbnailsPtr();
};

