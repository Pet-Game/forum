namespace PetGameForum.Data; 

public enum Permission {
	SeeModeratorArea,
	TempBanShort, //bans 1 month or shorter
	TempBanLong, //bans over 1 month
	PermaBan, //bans forever and IP bans
	DeletePosts, //make posts invisible
	RestorePosts, //restore deleted posts (only makes sense when they can see deleted information)
	NukePosts, //delete posts from database entirely
	MoveThreads, //move threads to different category
	ArchiveThreads, //archive threads
	EditThreads, //change topic/private/members of threads
	SeeAllThreads, //see private threads
	AssignRoles, //assign and remove existing roles from people
	EditRoles, //create and edit roles
	SeeDeletedInformation, //see deleted posts and other stuff thats still in the db
	SeeSecretUserInformation, //see extra information for users, like ban history
}