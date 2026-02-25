# MultiStreamViewer (WPF + LibVLCSharp)

Native Windows desktop app that displays multiple video streams in a grid (2x2 / 3x3).
Each tile can play a local demo video or connect to an RTSP stream.

## Run (EXE)

1. Extract `MultiStreamViewer_Publish.zip` to a folder.
2. Run `MultiStreamViewer.exe`.
3. Click **Load Demo** to start local MP4 demo videos in the grid.
4. To test RTSP: paste an `rtsp://...` URL into a tile and click **Connect**.

## Build from Source (Visual Studio)

1. Open `MultiStreamViewer.sln` in Visual Studio 2022.
2. Build/Run.

## Notes

- Demo videos are located in the `DemoVideos` folder next to the executable.
- Tile status shows: `LIVE` / `BUFFERING` / `PAUSED` / `STOPPED` / `FAILED`.