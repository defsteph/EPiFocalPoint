define(["dojo/on", "dojo/_base/declare", "dojo/aspect", "dijit/registry", "dijit/WidgetSet", "dijit/_Widget", "dijit/_TemplatedMixin", "dijit/_WidgetsInTemplateMixin",
    "epi/epi", "epi/shell/widget/_ValueRequiredMixin", "epi-cms/_ContentContextMixin", "epi/i18n!epi/cms/nls/focalpoint", "xstyle/css!./WidgetTemplate.css"],
function (on, declare, aspect, registry, WidgetSet, _Widget, _TemplatedMixin, _WidgetsInTemplateMixin, epi, _ValueRequiredMixin, _ContentContextMixin, resources) {
	return declare([_Widget, _TemplatedMixin, _WidgetsInTemplateMixin, _ValueRequiredMixin, _ContentContextMixin], {
		templateString: dojo.cache("focal-point.focalpoint", "WidgetTemplate.html"),
		imageUrl: null,
		intermediateChanges: true,
		resources: resources,
		value: null,
		constructor: function () {
			var context = this.getCurrentContext();
			this.imageUrl = context.previewUrl;
		},
		onChange: function (value) { },
		postCreate: function () {
			this.inherited(arguments);
		},
		startup: function () {
			this.initializeFocalPoint();
		},
		initializeFocalPoint: function () {
			if (this.hasCoordinates()) {
				this._setFocalPoint(this.value.x, this.value.y, true);
			} else {
				this.focalpoint.className = "empty";
			}
			if (this.readOnly) {
				this.clearbutton.disabled = true;
				this.focalpoint.className = "empty";
			}
			else {
				this.canvas.addEventListener("click", this.setFocalPoint.bind(this));
				this.clearbutton.addEventListener("click", this.clearCoordinates.bind(this));
			}
		},
		hasCoordinates: function () {
			if (!this.isValid() || !this.value || this.valueOf === "" || (typeof this.value === "object" && (isNaN(this.value.x) || isNaN(this.value.y)))) {
				return false;
			}
			return this.value.x !== undefined &&
				   this.value.y !== undefined &&
				   !isNaN(this.value.x) &&
				   !isNaN(this.value.y) &&
				   this.value.x !== 0 &&
				   this.value.y !== 0;
		},
		_setFocalPoint: function (x, y, isPercentage) {
			if (x === undefined || y === undefined) {
				return undefined;
			}
			var offsetX = x;
			var offsetY = y;
			if (isPercentage) {
				offsetX = this.image.width * x / 100;
				offsetY = this.image.height * y / 100;
			} else {
				x = (offsetX / this.image.width) * 100;
				y = (offsetY / this.image.height) * 100;
			}
			var focalPointSize = this.focalpoint.offsetWidth;
			this.focalpoint.style.left = offsetX - (focalPointSize / 2) + "px";
			this.focalpoint.style.top = offsetY - (focalPointSize / 2) + "px";
			this.focalpoint.className = "";
			return { y: y, x: x };
		},
		setFocalPoint: function (ev) {
			var coordinates = this._setFocalPoint(ev.offsetX, ev.offsetY, false);
			this._onCoordinateChanged(coordinates.x, coordinates.y);
			this.focalpoint.className = "";
		},
		_onCoordinateChanged: function (x, y) {
			if (!this._started) {
				return;
			}
			if (x === undefined || y === undefined) {
				return;
			}
			var value = { y: parseFloat(y), x: parseFloat(x) };
			this._set("value", value);
			this.onChange(value);
		},
		clearCoordinates: function () {
			this._set("value", null);
			this._setFocalPoint(50, 50, true);
			this.focalpoint.className = "empty";
			this.onChange(null);
		},
		isValid: function () {
			if (!this.value || this.value === "" || this.value == undefined || (typeof this.value === "object" && (isNaN(this.value.x) || isNaN(this.value.y)))) {
				return !this.required;
			}
			var isValidCoordinatesObject = this.value.x !== undefined &&
										   this.value.y !== undefined &&
										   !isNaN(this.value.x) &&
										   !isNaN(this.value.y) &&
										   this.value.x !== 0 &&
										   this.value.y !== 0;
			return isValidCoordinatesObject;
		},
		_setValueAttr: function (value) {
			if (value === this.value) {
				return;
			}
			this._set("value", value);
			var that = this;
			this.image.addEventListener("load", function () {
				that._setFocalPoint(value.x, value.y, true);
			});
			if (!value) {
				this.clearCoordinates();
				return;
			}
		}
	});
});