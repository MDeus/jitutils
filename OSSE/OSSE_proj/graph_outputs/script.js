
// https://stackoverflow.com/questions/41514967/yes-no-is-there-a-way-to-improve-mouse-dragging-with-pure-svg-tools/41518545#41518545

var dre = document.querySelectorAll(".node")
for (var i = 0; i < dre.length; i++) {
    var o = new Draggable(dre[i])
}

function Draggable(elem) {
    this.target = elem
    this.clickPoint = this.target.ownerSVGElement.createSVGPoint()
    this.lastMove = this.target.ownerSVGElement.createSVGPoint()
    this.currentMove = this.target.ownerSVGElement.createSVGPoint()
    this.target.addEventListener("mousedown", this)
    
    this.handleEvent = function(evt) {
        console.log(evt);
        console.log(this.target.ownerSVGElement)
        evt.preventDefault()
        this.clickPoint = globalToLocalCoords(evt.clientX, evt.clientY)
        this.target.classList.add("dragged")
        this.target.setAttribute("pointer-events", "none")
        this.target.ownerSVGElement.addEventListener("mousemove", this.move)
        this.target.ownerSVGElement.addEventListener("mouseup", this.endMove)
    }

    this.move = function(evt) {
        var p = globalToLocalCoords(evt.clientX, evt.clientY)
        this.currentMove.x = this.lastMove.x + (p.x - this.clickPoint.x)
        this.currentMove.y = this.lastMove.y + (p.y - this.clickPoint.y)
        this.target.setAttribute("transform", "translate(" + this.currentMove.x + "," + this.currentMove.y + ")")
    }.bind(this)

    this.endMove = function(evt) {
        this.lastMove.x = this.currentMove.x
        this.lastMove.y = this.currentMove.y
        this.target.classList.remove("dragged")
        this.target.setAttribute("pointer-events", "all")
        this.target.ownerSVGElement.removeEventListener("mousemove", this.move)
        this.target.ownerSVGElement.removeEventListener("mouseup", this.endMove)
    }.bind(this)

    function globalToLocalCoords(x, y) {
        var p = elem.ownerSVGElement.createSVGPoint()
        var m = elem.parentNode.getScreenCTM()
        console.log(p);
        p.x = x
        p.y = y
        return p.matrixTransform(m.inverse())
    }
}