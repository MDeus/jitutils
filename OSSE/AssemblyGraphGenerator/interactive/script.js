
// https://stackoverflow.com/questions/41514967/yes-no-is-there-a-way-to-improve-mouse-dragging-with-pure-svg-tools/41518545#41518545

// <script xlink:href="script.js" />
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
    this.edge;
    
    this.handleEvent = function(evt) {
        // this.target.appendChild(document.getElementById("edge3"))
        this.edge = document.getElementById("edge3")
        console.log(this.edge.getAttributeNS(null, "d"))
        // evt.target
        console.log(evt);
        this.edge = this.target.get
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

        // var p2 = globalToLocalCoords(this.edge.clientX, this.edge.clientY)
        // this.edge.setAttribute("x" , this.currentMove.x)
        // this.edge.setAttribute("y" , this.currentMove.y)

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
        // console.log(p);
        p.x = x
        p.y = y
        return p.matrixTransform(m.inverse())
    }
}

var svgDoc;
var selectedPoint = 0;
var dx = 0;
var dy = 0;

var example_shape;
var text_variables;
var control_lines;
var coords = [0, 0, 100, 100, 150, 20, 180, 150, 280, 100];

function init(evt) {
    if ( window.svgDocument == null ) {
        svgDoc = evt.target.ownerDocument;
    }  
    example_shape = svgDoc.getElementById('example_shape');
    controls_lines = [svgDoc.getElementById('control_line1'),
                      svgDoc.getElementById('control_line2')] ;
    text_variables = [];
    for (var i=1; i<=4; i++){
        text_variables.push(svgDoc.getElementById('variable'+i));
    }
};

function selectElement(evt, point){
    selectedPoint = point;
    control_point = evt.target.parentNode;
    dx = coords[selectedPoint*2] - evt.clientX;
    dy = coords[selectedPoint*2 + 1] - evt.clientY;
};

function drag(evt){
    if (selectedPoint === 0) { return; }
    
    var x = evt.clientX + dx;
    var y = evt.clientY + dy;
    coords[selectedPoint*2] = x;
    coords[selectedPoint*2 + 1] = y;
    
    var d = "M";
    for (var i=2; i<coords.length; i+=2){
        if (i === 4) { d += "C"; }
        d += " " + coords[i] + " " + coords[i+1];
    }
    example_shape.setAttributeNS(null, "d", d);
    control_point.setAttributeNS(null, "transform", "translate(" + x + "," + y + ")");
    text_variables[selectedPoint-1].firstChild.data = x + "," + y;
    
    // Control lines
    var n = Math.floor(selectedPoint/3);
    var i = (selectedPoint % 2) + 1;
    controls_lines[n].setAttributeNS(null, "x"+i, x);
    controls_lines[n].setAttributeNS(null, "y"+i, y);
    
};

function deselect(){
    selectedPoint = 0;
}