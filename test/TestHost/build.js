import { copyFile } from "node:fs/promises";

// Just emulate a build process...
await copyFile("Client/index.js", process.env.OUT_DIR + "index.min.js");
