/**
 * Each mine is 30 * 30 px, border is 1 px, totally is 32 * 32 px.
 * One group contains 20 * 20 mines. (640 * 640 px)
 */

// todo: http://vuejs.org/guide/components.html#Custom_Events

Vue.config.debug = true;

var MineGroup = Vue.extend({
    template: '<canvas width="640" height="640" :style="style" v-on:click.stop.prevent="click($event)"></canvas>',
    data: function () {
        return {
            ready: false,
        };
    },
    props: {
        mineGroupX: Number,
        mineGroupY: Number,
        offsetX: Number,
        offsetY: Number,
    },
    computed: {
        style: function () {
            return {
                position: "absolute",
                left: this.offsetX + "px",
                top: this.offsetY + "px",
            };
        },
    },
    compiled: function () {
        var self = this;
        setTimeout(function () {
            var data = self.$parent.get_init_data(this.mineGroupX, this.mineGroupY);

            self.init_canvas();
            // init canvas with data

            self.ready = true;
        });
    },
    events: {
        'click-callback': function (s_json) {
            if (!ready) return;

            // todo: update canvas

            return;
        }
    },
    methods: {
        click: function (e) {
            var x = e.offsetX % 32, y = e.offsetY % 32;
            if (0 == x || 31 == x || 0 == y || 31 == y) {
                return;  // click the border
            }
            x = Math.floor(e.offsetX / 32);
            y = Math.floor(e.offsetY / 32);
            console.log(x, y);
        },
        init_canvas: function () {
            var canvas = this.$el;
            var context = canvas.getContext("2d");

            context.fillStyle = '#D0D0D0';
            context.fillRect(0, 0, canvas.width, canvas.height);

            for (var i = 0; i <= 20; i++) {
                this.draw_line(context, 0, i * 32, canvas.width, i * 32);
                this.draw_line(context, i * 32, 0, i * 32, canvas.height);
            }

        },
        draw_line: function (context, begin_x, begin_y, end_x, end_y) {
            context.lineWidth = 1;
            context.beginPath();
            context.moveTo(begin_x, begin_y);
            context.lineTo(end_x, end_y);
            context.stroke();
            context.closePath();
        },
    },
});

var mine = Vue.component('mine', {
    template: '<div :style="main_style" draggable="true" @dragstart="drag_start" @drag.stop.prevent="drag_handler">' + // main div
        '<div width="100%" height="100%" :style="transform_style">' + // div use for drag
        '<mine-group v-for="g in groups" ' +
        ':mine-group-x="g.group_x"' +
        ':mine-group-y="g.group_y"' +
        ':offset-x="g.offset_x"' +
        ':offset-y="g.offset_y"' +
        '/>' +
        '</div>' +
        '</div>',
    data: function () {
        return {
            data: {},
            ready: false,
            init: {
                group_x: 0,
                group_y: 0,
            },
            main_style: {
                'z-index': "1",
                position: "absolute",
                overflow: 'hidden',
                height: '100%',
                width: '100%',
            },
            drag: {
                draging: false,
                drag_start_x: 0,
                drag_start_y: 0,
                drag_offset_x: 0,
                drag_offset_y: 0,
            },
        };
    },
    computed: {
        width: function () {
            var width = 0;
            if (this.$el) { width = this.$el.offsetWidth; }
            return width;
        },
        height: function () {
            var height = 0;
            if (this.$el) { res = this.$el.offsetHeight; }
            return height;
        },
        groups: function () {
            return [
                {
                    group_x: 0,
                    group_y: 0,
                    offset_x: 0,
                    offset_y: 0,
                },
                {
                    group_x: 0,
                    group_y: 0,
                    offset_x: 640,
                    offset_y: 640,
                },
            ];
        },
        transform_style: function () {
            return {
                transform: 'translate(' + this.drag.drag_offset_x + 'px, ' + this.drag.drag_offset_y + 'px)',
                position: 'absolute',
                top: '0px',
                left: '0px',
            };
        },
    },
    methods: {
        send_to_server: function (s_json) {
            console.warn('init send_to_server called');
        },
        receive_from_server: function (s_json) {
            console.log(s_json);

            // todo, send to callback_click_result or callback_init_data
        },
        send_to_server_factory: function (hub) {
            return function (s_json) {
                if (hub == undefined) {
                    console.warn('hub is undefined');
                    return;
                }
                hub.server.sendToServer(s_json);
            };
        },
        callback_click_result: function (s_json) {
            // left click & right click
            // is mine or not

            this.$broadcast('click-callback', s_json);

            return;
        },
        callback_init_data: function (s_json) {
            // just update data
            // $watch will be fired in get_init_data

            return;
        },
        get_init_data: function (group_x, group_y) {
            // set watch for given group


            return '';
        },
        drag_start: function (e) {
            console.log("drag start");
            var x = e.offsetX, y = e.offsetY;

            this.drag.drag_prev_x = x;
            this.drag.drag_prev_y = y;
        },
        drag_handler: function (e) {
            var x = e.offsetX, y = e.offsetY;

            if (0 == x && 0 == y) {
                console.log("drag end");
                return;
            }

            this.drag.drag_offset_x += x - this.drag.drag_prev_x;
            this.drag.drag_offset_y += y - this.drag.drag_prev_y;

            this.drag.drag_prev_x = x;
            this.drag.drag_prev_y = y;
        },
    },
    compiled: function () {
        var self = this;

        var mine_hub = $.connection.mineHub;
        mine_hub.client.sendToClient = self.receive_from_server;

        $.connection.hub.start().done(function () {
            console.assert(mine_hub != undefined, "hub object is null");
            if (mine_hub) {
                self.send_to_server = self.send_to_server_factory(mine_hub);
                self.ready = true;
            } else {
                console.warn("hub connection not started, please refresh the page.");
            }
        });
    },
    components: {
        'mine-group': MineGroup,
    },
});

(function () {
    new Vue({
        el: '#mine',
    });
})();