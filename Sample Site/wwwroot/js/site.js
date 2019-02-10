// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var ChartBackground = new
{
    Red : 'rgba(255, 99, 132, 0.2)',
    Blue : 'rgba(54, 162, 235, 0.2)',
    Yellow : 'rgba(255, 206, 86, 0.2)',
    Green : 'rgba(75, 192, 192, 0.2)',
    Purple : 'rgba(153, 102, 255, 0.2)',
    Orange : 'rgba(255, 159, 64, 0.2)'
};

var ChartBorder = new
{
    Red : 'rgba(255,99,132,1)',
    Blue : 'rgba(54, 162, 235, 1)',
    Yellow : 'rgba(255, 206, 86, 1)',
    Green : 'rgba(75, 192, 192, 1)',
    Purple : 'rgba(153, 102, 255, 1)',
    Orange : 'rgba(255, 159, 64, 1)'
};

function barChart(canvas,labels, data)
{
    var ctx = document.getElementById(canvas).getContext('2d');
    var myChart = new Chart(ctx,
        {
            type: 'bar',
            data: {
                labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
                datasets: [{
                    label: '# of Votes',
                    data: [12, 19, 3, 5, 2, 3],
                    backgroundColor: [
                                
                    ],
                    borderColor: [

                    ],
                    borderWidth: 1
                }]
            },
            options:
            {
                scales: { yAxes: [{ ticks: { beginAtZero:true } }] }
            }
        });
}