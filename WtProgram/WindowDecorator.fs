namespace Bemo
open System
open System.Drawing

type WindowDecorator = {
    windowBounds: Rect
    monitorBounds: List2<Rect>
    decoratorHeight : int
    decoratorHeightOffset : int
    decoratorIndentFlipped : int
    decoratorIndentNormal : int
    } with

    member private this.screenRegion =
        this.monitorBounds.fold (Rgn()) <| fun screenRegion monitorBounds ->
            let monitorRegion = Rgn(monitorBounds)
            screenRegion.union(monitorRegion)

    member private this.indent(isCentered) = if isCentered then this.decoratorIndentFlipped else this.decoratorIndentNormal

    member private this.outsideBounds =
        let rect = this.windowBounds
        let indent = this.indent(false)
        Rect(
            Pt(rect.x + indent, rect.y - this.decoratorHeight + this.decoratorHeightOffset),
            Sz(rect.width - 2 * indent, this.decoratorHeight)
        )

    member this.insideBounds = 
        let rect = this.windowBounds
        let indent = this.indent(true)
        Rect(
            Pt(rect.x + indent, rect.y - 1), // offset by one so it covers edge case #741
            Sz(rect.width - 2 * indent, this.decoratorHeight)
        )

    member this.shouldShowInside = 
        let decoratorOutsideRegion = Rgn(this.outsideBounds)
        let decoratorInsideRegion = Rgn(this.insideBounds)
        this.monitorBounds.any <| fun monitorBounds ->
            let monitorRegion = Rgn(monitorBounds)
            let onMonitorInsideRegion = monitorRegion.intersect(decoratorInsideRegion)
            let onMonitorOutsideRegion = monitorRegion.intersect(decoratorOutsideRegion)
            onMonitorInsideRegion.box.height > onMonitorOutsideRegion.box.height

    member this.bounds : Rect =
        if this.shouldShowInside then this.insideBounds else this.outsideBounds
    

