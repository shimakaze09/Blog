let homeApp = new Vue({
    el: '#vue-app',
    data: {
        poem: {},
        hitokoto: {},
        poemSimple: '',
        chartTypes: ['bubble', 'bar'],
        currentChartTypeIndex: 0,
        currentChart: null
    },
    computed: {
        chartElem() {
            return document.getElementById('myChart')
        }
    },
    created() {
        fetch('http://dc.sblt.deali.cn:9800/poem/simple')
            .then(res => res.text()).then(data => this.poemSimple = data)
        fetch('http://dc.sblt.deali.cn:9800/poem/tang')
            .then(res => res.json())
            .then(data => {
                this.poem = data.data
            })
        fetch('http://dc.sblt.deali.cn:9800/hitokoto/get')
            .then(res => res.json())
            .then(data => {
                this.hitokoto = data.data[0]
            })
    },
    mounted() {
        this.loadChart()
    },
    methods: {
        /**
         * Generates a random RGB color string, e.g., "rgb(123,123,123)"
         * @returns {string}
         */
        randomRGB() {
            return 'rgb(' + this.randomColorArray().join(',') + ')'
        },
        // Generates a random RGBA string, e.g., "rgba(123,123,123,0.2)"
        randomRGBA(a) {
            return this.convertRGBA(this.randomColorArray(), a)
        },
        // Converts RGB array to RGBA string
        convertRGBA(rgbArray, a) {
            let color = Array.from(rgbArray)
            color.push(a)
            return 'rgba(' + color.join(',') + ')'
        },
        randomColorArray() {
            return [
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255),
            ]
        },
        switchChartType() {
            if (this.currentChartTypeIndex >= this.chartTypes.length - 1)
                this.currentChartTypeIndex = 0
            else
                this.currentChartTypeIndex++
            if (this.currentChart)
                this.currentChart.destroy()
            this.chartElem.setAttribute('style', '')
            this.loadChart()
        },
        loadChart() {
            let chartType = this.chartTypes[this.currentChartTypeIndex]
            switch (chartType) {
                case 'bubble':
                    this.loadBubbleChart()
                    break
                case 'bar':
                    this.loadBarChart()
                    break
                default:
            }
        },
        loadBubbleChart() {
            fetch('/Api/Category/WordCloud').then(res => res.json())
                .then(res => {
                    let datasets = []
                    res.data.forEach(item => {
                        let color = this.randomColorArray()
                        datasets.push({
                            label: item.name,
                            data: [{
                                x: Math.round(Math.random() * 50),
                                y: Math.round(Math.random() * 50),
                                r: item.value
                            }],
                            backgroundColor: this.convertRGBA(color, 0.2),
                            borderColor: this.convertRGBA(color, 1),
                            borderWidth: 1
                        })
                    })

                    let data = {
                        datasets: datasets
                    };
                    let config = {
                        type: 'bubble',
                        data: data,
                        options: {
                            maintainAspectRatio: false,
                        }
                    };

                    this.currentChart = new Chart(this.chartElem, config)
                    this.currentChart.resize(null, 400)
                })
        },
        loadBarChart() {
            fetch('/Api/Category/WordCloud').then(res => res.json())
                .then(res => {
                    let labels = []
                    let values = []
                    let backgroundColors = []
                    let borderColors = []
                    res.data.forEach(item => {
                        labels.push(item.name)
                        values.push(item.value)
                        let color = this.randomColorArray()
                        backgroundColors.push(this.convertRGBA(color, 0.2))
                        borderColors.push(this.convertRGBA(color, 1))
                    })
                    let data = {
                        labels: labels,
                        datasets: [{
                            label: '# of Votes',
                            data: values,
                            backgroundColor: backgroundColors,
                            borderColor: borderColors,
                            borderWidth: 1
                        }]
                    }
                    let config = {
                        type: 'bar',
                        data: data,
                        options: {
                            maintainAspectRatio: false,
                        }
                    }

                    this.currentChart = new Chart(this.chartElem, config)
                    this.currentChart.resize(null, 400)
                })
        }
    }
})
