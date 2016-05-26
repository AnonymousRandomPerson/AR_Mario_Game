using Sanford.Collections.Generic;

/// <summary>
/// Keeps track of which parts of the paths to outline.
/// </summary>
public class PathRendererList {

	/// <summary> The maximum number of lines to render. </summary>
	private const int CAPACITY = 10;

	/// <summary> All currently rendered path lines. </summary>
	private Deque<PathRenderer> pathList;

	/// <summary> Whether the path renderer list is enabled. </summary>
	private bool outlinePath;

	/// <summary>
	/// Creates a new path renderer list.
	/// </summary>
	/// <param name="startPath">The start of the path being rendered.</param>
	public PathRendererList(PathComponent startPath) {
		pathList = new Deque<PathRenderer>();
		outlinePath = LevelManager.Instance.outlinePath;
		Init(startPath);
	}

	/// <summary>
	/// Fills the path list with the first lines in the path.
	/// </summary>
	/// <param name="startPath">The start of the path being rendered.</param>
	public void Init(PathComponent startPath) {
		if (pathList.Count > 0) {
			foreach (PathRenderer pathRenderer in pathList) {
				pathRenderer.SetVisible(false);
			}
			pathList.Clear();
		}
		if (startPath != null) {
			PathRenderer current = startPath.firstLine;
			while (pathList.Count < CAPACITY && current != null) {
				PushFront(current);
				current = current.nextLine;
			}
		}
	}

	/// <summary>
	/// Puts a path renderer at the front of the list. Evicts the back of the list if at capacity.
	/// </summary>
	/// <param name="pathRenderer">The path renderer to put at the front of the list.</param>
	private void PushFront(PathRenderer pathRenderer) {
		if (pathList.Count >= CAPACITY) {
			pathList.PopBack().SetVisible(false);
		}
		pathList.PushFront(pathRenderer);
		pathRenderer.SetVisible(outlinePath);
	}

	/// <summary>
	/// Puts a path renderer at the back of the list. Evicts the front of the list if at capacity.
	/// </summary>
	/// <param name="pathRenderer">The path renderer to put at the back of the list.</param>
	private void PushBack(PathRenderer pathRenderer) {
		if (pathList.Count >= CAPACITY) {
			pathList.PopFront().SetVisible(false);
		}
		pathList.PushBack(pathRenderer);
		pathRenderer.SetVisible(outlinePath);
	}

	/// <summary>
	/// Checks for changing the displayed line based on player movement.
	/// </summary>
	/// <param name="player">The moving player.</param>
	/// <param name="forward">Whether the player is moving forward.</param>
	public void CheckList(Player player, bool forward) {
		if (pathList != null) {
			if (forward) {
				CheckForward(player);
			} else {
				CheckBackward(player);
			}
		}
	}

	/// <summary>
	/// Checks for changing the displayed line when the player moves forwards.
	/// </summary>
	/// <param name="player">The player moving forward.</param>
	private void CheckForward(Player player) {
		PathRenderer nextLine = pathList.PeekFront().nextLine;
		if (nextLine != null && nextLine.GetBackDistance(player) < pathList.PeekBack().GetFrontDistance(player)) {
			PushFront(nextLine);
		}
	}

	/// <summary>
	/// Checks for changing the displayed line when the player moves backwards.
	/// </summary>
	/// <param name="player">The player moving backwards.</param>
	private void CheckBackward(Player player) {
		PathRenderer prevLine = pathList.PeekBack().prevLine;
		if (prevLine != null && prevLine.GetFrontDistance(player) < pathList.PeekFront().GetBackDistance(player)) {
			PushBack(prevLine);
		}
	}
}
