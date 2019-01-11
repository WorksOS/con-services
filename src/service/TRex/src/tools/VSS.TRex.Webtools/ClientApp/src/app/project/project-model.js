export class ProjectExtents {
    constructor(minX, minY, maxX, maxY) {
        this.minX = 0;
        this.minY = 0;
        this.maxX = 0;
        this.maxY = 0;
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }
    toString() {
        return `MinX:${this.minX.toFixed(3)}, MaxX:${this.maxX.toFixed(3)}, MinY:${this.minY.toFixed(3)}, MaxY:${this.maxY.toFixed(3)}`;
    }
    Set(minX, minY, maxX, maxY) {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }
    sizeX() { return this.maxX - this.minX; }
    sizeY() { return this.maxY - this.minY; }
    centerX() { return (this.maxX + this.minX) / 2; }
    centerY() { return (this.maxY + this.minY) / 2; }
    shrink(factorX, factorY) {
        let curSizeX = this.sizeX();
        let curSizeY = this.sizeY();
        this.minX += curSizeX * (factorX / 2);
        this.minY += curSizeY * (factorY / 2);
        this.maxX -= curSizeX * (factorX / 2);
        this.maxY -= curSizeY * (factorY / 2);
    }
    expand(factorX, factorY) {
        let curSizeX = this.sizeX();
        let curSizeY = this.sizeY();
        this.minX -= curSizeX * (factorX / 2);
        this.minY -= curSizeY * (factorY / 2);
        this.maxX += curSizeX * (factorX / 2);
        this.maxY += curSizeY * (factorY / 2);
    }
    panByFactor(dx_factor, dy_factor) {
        this.panByDelta(dx_factor * this.sizeX(), dy_factor * this.sizeY());
    }
    panByDelta(dx, dy) {
        this.minX += dx;
        this.minY += dy;
        this.maxX += dx;
        this.maxY += dy;
    }
    setCenterPosition(cx, cy) {
        this.panByDelta(cx - this.centerX(), cy - this.centerY());
    }
}
export class DesignDescriptor {
    constructor() {
        this.designId = "";
        this.fileSpace = "";
        this.fileSpaceID = "";
        this.folder = "";
        this.fileName = "";
        this.offset = 0;
    }
}
export class Design {
    constructor() {
        this.id = "";
        this.designDescriptor = new DesignDescriptor();
        this.extents = new ProjectExtents(0, 0, 0, 0);
    }
}
export class MachineDesign {
    constructor() {
        this.name = "";
    }
}
export class SiteProofingRun {
    constructor() {
        this.name = "";
        this.startDate = new Date();
        this.endDate = new Date();
    }
}
export class SurveyedSurface {
    constructor() {
        this.id = "";
        this.designDescriptor = new DesignDescriptor();
        this.asAtDate = new Date();
        this.extents = new ProjectExtents(0, 0, 0, 0);
    }
}
export class Machine {
    constructor() {
        this.id = "";
    }
}
export class MachineEventType {
}
export class XYZS {
}
//# sourceMappingURL=project-model.js.map