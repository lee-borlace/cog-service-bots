# Notes
- This uses person groups instead of large person groups due to the limited .Net Core library. Should be updated to use large person groups when a new package is available. Or updated to use REST API for large person groups.
- To create a person group, suggest you use the XAML test harness at https://github.com/Microsoft/Cognitive-Face-Windows. This will create LARGE person groups (see the previous point). It's easy enough to make a few changes (to FaceIdentificationPage.xaml) to make it use person groups instead of large person groups.
- Once you have the person group ID to be used for IDing the user, add this to Face.PersonGroupId in config.

# Issues
- Need to get bot chat to reset when user changes without throwing error. Currently we just send an event to the bot so it can pick up the new user ID / user name, but the actual user ID / name used by the bot doesn't change. Re-calling the bot init code causes errors if the bot has already been initialised. Have tried things like nulling out the directline object, clearing out the web chat div but it doesn't help.